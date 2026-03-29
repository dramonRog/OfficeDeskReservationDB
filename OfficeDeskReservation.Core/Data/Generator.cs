using Bogus;
using OfficeDeskReservationDB.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace OfficeDeskReservationDB.Data
{
    public class Generator
    {
        private readonly AppDbContext _context;
        private const int BatchSize = 10000; 

        public Generator(AppDbContext context)
        {
            _context = context;
        }

        private void SaveAndClear()
        {
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
        }

        public void GenerateDesks(int count, AppDbContext context)
        {
            var roomIds = context.Rooms.AsNoTracking().Select(r => r.Id).ToList();
            if (!roomIds.Any()) return;

            HashSet<string> existingNames = context.Desks.AsNoTracking().Select(d => d.Name).ToHashSet();

            var faker = new Faker();
            for (int i = 1; i <= count; i++)
            {
                string name;
                do
                {
                    name = $"D-{faker.Random.Number(1, 1000000)}-{faker.Commerce.ProductAdjective()}";
                } while (existingNames.Contains(name));

                existingNames.Add(name);

                context.Desks.Add(new Desk
                {
                    Name = name,
                    IsActive = faker.Random.Bool(0.8f),
                    RoomId = faker.PickRandom(roomIds)
                });

                if (i % BatchSize == 0)
                {
                    Console.WriteLine($"Desks: Saved {i}...");
                    SaveAndClear();
                }
            }
            SaveAndClear();
        }

        public void GenerateUsers(int count, AppDbContext context)
        {
            var roleIds = context.Roles.AsNoTracking().Select(r => r.Id).ToList();
            var deptIds = context.Departments.AsNoTracking().Select(d => d.Id).ToList();
            HashSet<string> existingEmails = context.Users.AsNoTracking().Select(u => u.Email).ToHashSet();

            var faker = new Faker();
            for (int i = 1; i <= count; i++)
            {
                string email;
                do
                {
                    email = faker.Internet.Email();
                } while (existingEmails.Contains(email));
                existingEmails.Add(email);

                context.Users.Add(new User
                {
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    Email = email,
                    RoleId = faker.PickRandom(roleIds),
                    DepartmentId = faker.PickRandom(deptIds),
                    PasswordHash = "hashed_dummy_password" 
                });

                if (i % BatchSize == 0)
                {
                    Console.WriteLine($"Users: Saved {i}...");
                    SaveAndClear();
                }
            }
            SaveAndClear();
        }

        public void GenerateReservations(int count, AppDbContext context)
        {
            var userIds = context.Users.AsNoTracking().Select(u => u.Id).ToList();
            var deskIds = context.Desks.AsNoTracking().Select(d => d.Id).ToList();

            var deskAvailability = deskIds.ToDictionary(id => id, _ => DateTime.Now.AddDays(1).Date.AddHours(8));

            var faker = new Faker();
            for (int i = 1; i <= count; i++)
            {
                int deskId = faker.PickRandom(deskIds);
                DateTime start = deskAvailability[deskId];
                DateTime end = start.AddHours(2);
                deskAvailability[deskId] = start.AddHours(4); 

                context.Reservations.Add(new Reservation
                {
                    UserId = faker.PickRandom(userIds),
                    DeskId = deskId,
                    Status = "Confirmed",
                    StartTime = start,
                    EndTime = end
                });

                if (i % BatchSize == 0)
                {
                    Console.WriteLine($"Reservations: Saved {i}...");
                    SaveAndClear();
                }
            }
            SaveAndClear();
        }

        public void GenerateIssues(int count, AppDbContext context)
        {
            var userIds = context.Users.AsNoTracking().Select(u => u.Id).ToList();
            var deskIds = context.Desks.AsNoTracking().Select(d => d.Id).ToList();
            var faker = new Faker();

            for (int i = 1; i <= count; i++)
            {
                context.Issues.Add(new Issue
                {
                    UserId = faker.PickRandom(userIds),
                    DeskId = faker.PickRandom(deskIds),
                    IsResolved = faker.Random.Bool(),
                    ReportedAt = faker.Date.Past(1),
                    Description = faker.Lorem.Sentence()
                });

                if (i % BatchSize == 0)
                {
                    Console.WriteLine($"Issues: Saved {i}...");
                    SaveAndClear();
                }
            }
            SaveAndClear();
        }

        public void GenerateDeskEquipments(int count, AppDbContext context)
        {
            Console.WriteLine("Loading existing equipment assignments from DB...");
            var deskIds = context.Desks.AsNoTracking().Select(d => d.Id).ToList();
            var eqIds = context.Equipments.AsNoTracking().Select(e => e.Id).ToList();

            var usedPairs = context.DeskEquipments
                .AsNoTracking()
                .Select(de => new { de.DeskId, de.EquipmentId })
                .AsEnumerable()
                .Select(x => (x.DeskId, x.EquipmentId))
                .ToHashSet();

            int maxPossible = deskIds.Count * eqIds.Count;
            if (count > (maxPossible - usedPairs.Count))
            {
                count = maxPossible - usedPairs.Count;
                Console.WriteLine($"Adjusting count to {count} due to unique constraint limits.");
            }

            var faker = new Faker();
            int generatedSoFar = 0;

            while (generatedSoFar < count)
            {
                int dId = faker.PickRandom(deskIds);
                int eId = faker.PickRandom(eqIds);

                if (usedPairs.Add((dId, eId)))
                {
                    context.DeskEquipments.Add(new DeskEquipment
                    {
                        DeskId = dId,
                        EquipmentId = eId,
                        Quantity = faker.Random.Int(1, 3)
                    });

                    generatedSoFar++;

                    if (generatedSoFar % BatchSize == 0)
                    {
                        Console.WriteLine($"Equipment: Progress {generatedSoFar} / {count}...");
                        SaveAndClear();
                    }
                }
            }
            SaveAndClear();
            Console.WriteLine("[SUCCESS] Desk Equipments generated.");
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