using Bogus;
using OfficeDeskReservationDB.Models;
using System.Security.Cryptography;
using System.Text;

namespace OfficeDeskReservationDB.Data
{
    public class Generator
    {
        private readonly AppDbContext _context;

        public Generator(AppDbContext context)
        {
            _context = context;
        }

        public void GenerateDesks(int count, AppDbContext context)
        {
            var roomIds = context.Rooms.Select(r => r.Id).ToList();
            if (!roomIds.Any())
            {
                Console.WriteLine("Error: Rooms table is empty!!!");
                return;
            }

            HashSet<string> existingNames = context.Desks.Select(d => d.Name).ToHashSet();

            Faker<Desk> deskFaker = new Faker<Desk>()
                .RuleFor(d => d.Name, f => {
                    string name;
                    do
                    {
                        name = $"D-{f.Random.Number(1, 9999)}-{f.Commerce.ProductName()}";
                    } 
                    while(existingNames.Contains(name));

                    existingNames.Add(name);
                    return name;
                })
                .RuleFor(d => d.IsActive, f => f.Random.Bool(0.8f))
                .RuleFor(d => d.RoomId, f => f.PickRandom(roomIds));

            List<Desk> generatedDesks = deskFaker.Generate(count);
            context.AddRange(generatedDesks);
            context.SaveChanges();
        }

        public void GenerateIssues(int count, AppDbContext context)
        {
            var userIds = context.Users.Select(u => u.Id).ToList();
            if (!userIds.Any())
            {
                Console.WriteLine("Users table is empty!!!");
                return;
            }

            var deskIds = context.Desks.Select(d => d.Id).ToList();
            if (!deskIds.Any())
            {
                Console.WriteLine("Desks table is empty!!!");
                return;
            }

            Faker<Issue> issueFaker = new Faker<Issue>()
                .RuleFor(i => i.UserId, f => f.PickRandom(userIds))
                .RuleFor(i => i.DeskId, f => f.PickRandom(deskIds))
                .RuleFor(i => i.IsResolved, f => f.Random.Bool(0.7f))
                .RuleFor(i => i.ReportedAt, f => f.Date.Past(1))
                .RuleFor(i => i.Description, f => f.PickRandom(new string[]
                {
                    "The monitor is intermittent and flickering.",
                    "The desk is unstable and wobbly.",
                    "The left power outlet is out.",
                    "The wheel on the chair at this desk is broken.",
                    "I am reporting a missing HDMI cable.",
                    "The keyboard has a stuck spacebar.",
                    "The internet outlet on the wall is not working."
                }));

            List<Issue> generatedIssues = issueFaker.Generate(count);
            context.Issues.AddRange(generatedIssues);
            context.SaveChanges();
        }

        public void GenerateUsers(int count, AppDbContext context)
        {
            var roleIds = context.Roles.Select(r => r.Id).ToList();
            if (!roleIds.Any())
            {
                Console.WriteLine("Roles table is empty!!!");
                return;
            }

            HashSet<string> existingEmails = context.Users.Select(u => u.Email).ToHashSet(); 
            var departmentIds = context.Departments.Select(d => d.Id).ToList();

            if (!departmentIds.Any())
            {
                Console.WriteLine("Departments table is empty!!!");
                return;
            }

            Faker<User> userFaker = new Faker<User>()
                .RuleFor(u => u.RoleId, f => f.PickRandom(roleIds))
                .RuleFor(u => u.DepartmentId, f => f.PickRandom(departmentIds))
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f =>
                {
                    string email;
                    do
                    {
                        email = f.Internet.Email();
                    } while (existingEmails.Contains(email));

                    existingEmails.Add(email);
                    return email;
                })
                .RuleFor(u => u.PasswordHash, f => HashPassword(f.Internet.Password()));

            List<User> generatedUsers = userFaker.Generate(count);
            context.Users.AddRange(generatedUsers);
            context.SaveChanges();
        }

        public void GenerateDeskEquipments(int count, AppDbContext context)
        {
            var deskIds = context.Desks.Select(d => d.Id).ToList();
            if (!deskIds.Any())
            {
                Console.WriteLine("Desks table is empty!!!");
                return;
            }

            var equipmentIds = context.Equipments.Select(e => e.Id).ToList();
            if (!equipmentIds.Any())
            {
                Console.WriteLine("Equipments table is empty!!!");
                return;
            }

            HashSet<(int DeskId, int EquipmentId)> usedPairs = context.DeskEquipments
                .Select(d => new { d.DeskId, d.EquipmentId })
                .ToList()
                .Select(x => (x.DeskId, x.EquipmentId))
                .ToHashSet();

            int maxCombinations = deskIds.Count * equipmentIds.Count;
            int availableSlots = maxCombinations - usedPairs.Count;

            if (count > availableSlots)
            {
                Console.WriteLine($"Warning: Requested {count} entries, but only {maxCombinations} unique pairs are possible! Reducing count.");
                count = availableSlots;
            }

            List<DeskEquipment> generatedDeskEquipments = new List<DeskEquipment>();

            Faker<DeskEquipment> deskEquipmentFaker = new Faker<DeskEquipment>()
                .RuleFor(d => d.DeskId, f => f.PickRandom(deskIds))
                .RuleFor(d => d.EquipmentId, f => f.PickRandom(equipmentIds))
                .RuleFor(d => d.Quantity, f => f.Random.Int(min: 1, max: 10));

            while (generatedDeskEquipments.Count < count)
            {
                DeskEquipment newEntry = deskEquipmentFaker.Generate();

                if (usedPairs.Add((DeskId: newEntry.DeskId, EquipmentId: newEntry.EquipmentId)))
                    generatedDeskEquipments.Add(newEntry);
            }

            context.DeskEquipments.AddRange(generatedDeskEquipments);
            context.SaveChanges();
        }

        public void GenerateReservations(int count, AppDbContext context)
        {
            var userIds = context.Users.Select(u => u.Id).ToList();
            if (!userIds.Any())
            {
                Console.WriteLine("Users table is empty!!!");
                return;
            }

            var deskIds = context.Desks.Select(d => d.Id).ToList();
            if (!deskIds.Any())
            {
                Console.WriteLine("Desks table is empty!!!");
                return;
            }

            Dictionary<int, DateTime> deskAvailability = deskIds.ToDictionary(
                id => id,
                id => context.Reservations
                    .Where(r => r.DeskId == id)
                    .Max(r => (DateTime?)r.EndTime) ?? DateTime.Now.AddDays(1).Date.AddHours(8)
            );

            Faker<Reservation> reservationFaker = new Faker<Reservation>()
                .RuleFor(r => r.UserId, f => f.PickRandom(userIds))
                .RuleFor(r => r.DeskId, f => f.PickRandom(deskIds))
                .RuleFor(r => r.Status, f => f.PickRandom(new[] { "Confirmed", "Pending", "Completed" }))
                .RuleFor(r => r.StartTime, (f, r) => deskAvailability[r.DeskId])
                .RuleFor(r => r.EndTime, (f, r) =>
                {
                    DateTime end = r.StartTime.AddHours(2);
                    deskAvailability[r.DeskId] = r.StartTime.AddHours(4);

                    return end;
                });

            List<Reservation> generatedReservations = reservationFaker.Generate(count);
            context.Reservations.AddRange(generatedReservations);
            context.SaveChanges();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}

