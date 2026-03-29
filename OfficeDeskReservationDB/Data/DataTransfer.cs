using OfficeDeskReservationDB.Models; 
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Driver;
using System.Diagnostics;

namespace OfficeDeskReservationDB.Data
{
    public class DatabaseBackup
    {
        public List<Location>? Locations { get; set; } = new();
        public List<Floor>? Floors { get; set; } = new();
        public List<Room>? Rooms { get; set; } = new();
        public List<Equipment>? Equipments { get; set; } = new();
        public List<Role>? Roles { get; set; } = new();
        public List<Department>? Departments { get; set; } = new();
        public List<User>? Users { get; set; } = new();
        public List<Desk>? Desks { get; set; } = new();
        public List<DeskEquipment>? DeskEquipments { get; set; } = new();
        public List<Reservation>? Reservations { get; set; } = new();
        public List<Issue>? Issues { get; set; } = new();
    }

    public static class DataTransfer
    {
        public static void ExportDatabaseToJson(AppDbContext context, string filePath)
        {
            Console.WriteLine("\n--- EXPORTING DATA ---");
            Console.WriteLine("Downloading data from the database...");

            var backup = new DatabaseBackup
            {
                Locations = context.Locations.AsNoTracking().ToList(),
                Floors = context.Floors.AsNoTracking().ToList(),
                Rooms = context.Rooms.AsNoTracking().ToList(),
                Equipments = context.Equipments.AsNoTracking().ToList(),
                Roles = context.Roles.AsNoTracking().ToList(),
                Departments = context.Departments.AsNoTracking().ToList(),
                Users = context.Users.AsNoTracking().ToList(),
                Desks = context.Desks.AsNoTracking().ToList(),
                DeskEquipments = context.DeskEquipments.AsNoTracking().ToList(),
                Reservations = context.Reservations.AsNoTracking().ToList(),
                Issues = context.Issues.AsNoTracking().ToList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string jsonString = JsonSerializer.Serialize(backup, options);
            File.WriteAllText(filePath, jsonString);

            Console.WriteLine($"[SUCCESS] Full database exported to: {filePath}");
        }

        public static void ImportDatabaseFromJson(AppDbContext context, string filePath)
        {
            Console.WriteLine("\n--- IMPORTING DATA ---");
            Console.WriteLine("Start import database from file JSON...");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("[ERROR] File doesn't exist!!!");
                return;
            }

            string jsonString = File.ReadAllText(filePath);
            var backup = JsonSerializer.Deserialize<DatabaseBackup>(jsonString);

            if (backup != null)
            {
                int addedUsers = 0, addedDesks = 0, addedRes = 0, addedIssues = 0, addedEq = 0;

                if (backup.Users != null)
                {
                    foreach (var user in backup.Users)
                    {
                        if (!context.Users.Any(u => u.Email == user.Email))
                        {
                            user.Id = 0;
                            context.Users.Add(user);
                            addedUsers++;
                        }
                    }
                }

                if (backup.Desks != null)
                {
                    foreach (var desk in backup.Desks)
                    {
                        if (!context.Desks.Any(d => d.Name == desk.Name))
                        {
                            desk.Id = 0;
                            context.Desks.Add(desk);
                            addedDesks++;
                        }
                    }
                }

                if (addedUsers > 0 || addedDesks > 0)
                {
                    context.SaveChanges();
                }

                if (backup.Reservations != null)
                {
                    foreach (var res in backup.Reservations)
                    {
                        if (!context.Reservations.Any(r => r.DeskId == res.DeskId && r.StartTime == res.StartTime))
                        {
                            if (context.Desks.Any(d => d.Id == res.DeskId) && context.Users.Any(u => u.Id == res.UserId))
                            {
                                res.Id = 0;
                                context.Reservations.Add(res);
                                addedRes++;
                            }
                        }
                    }
                }

                if (backup.Issues != null)
                {
                    foreach (var issue in backup.Issues)
                    {
                        if (!context.Issues.Any(i => i.Description == issue.Description && i.ReportedAt == issue.ReportedAt))
                        {
                            if (context.Users.Any(u => u.Id == issue.UserId) && context.Desks.Any(d => d.Id == issue.DeskId))
                            {
                                issue.Id = 0;
                                context.Issues.Add(issue);
                                addedIssues++;
                            }
                        }
                    }
                }

                if (backup.DeskEquipments != null)
                {
                    foreach (var de in backup.DeskEquipments)
                    {
                        if (!context.DeskEquipments.Any(x => x.DeskId == de.DeskId && x.EquipmentId == de.EquipmentId))
                        {
                            if (context.Desks.Any(d => d.Id == de.DeskId) && context.Equipments.Any(e => e.Id == de.EquipmentId))
                            {
                                context.DeskEquipments.Add(de);
                                addedEq++;
                            }
                        }
                    }
                }

                context.SaveChanges();

                Console.WriteLine($"[SUCCESS] Skipped duplicates and invalid relations. Saved NEW records:");
                Console.WriteLine($"- {addedUsers} Users");
                Console.WriteLine($"- {addedDesks} Desks");
                Console.WriteLine($"- {addedRes} Reservations");
                Console.WriteLine($"- {addedIssues} Issues");
                Console.WriteLine($"- {addedEq} Desk Equipments");
            }
        }

        public static void TransformToNoSql(AppDbContext context)
        {
            Console.WriteLine("\n--- TRANSFORMING SQL TO MongoDB DATABASE ---");
            Console.WriteLine("Fetching and denormalizing entire relational database...");

            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("OfficeDeskReservationDB");

            database.DropCollection("Users");
            database.DropCollection("Desks");
            database.DropCollection("Reservations");
            database.DropCollection("Issues");

            var usersCollection = database.GetCollection<NoSqlUser>("Users");
            var desksCollection = database.GetCollection<NoSqlDesk>("Desks");
            var reservationsCollection = database.GetCollection<NoSqlReservation>("Reservations");
            var issuesCollection = database.GetCollection<NoSqlIssue>("Issues");

            List<User> sqlUsers = context.Users.AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Department)
                .ToList();

            List<NoSqlUser> noSqlUsers = new List<NoSqlUser>();

            foreach (var user in sqlUsers)
            {
                noSqlUsers.Add(new NoSqlUser
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    RoleName = user.Role.Name,
                    DepartmentName = user.Department.Name
                });
            }

            if (noSqlUsers.Any())
                usersCollection.InsertMany(noSqlUsers);


            List<Desk> sqlDesks = context.Desks.AsNoTracking()
                .Include(d => d.Room)
                    .ThenInclude(r => r.Floor)
                        .ThenInclude(r => r.Location)
                .Include(d => d.DeskEquipments)
                    .ThenInclude(de => de.Equipment)
                .ToList();

            List<NoSqlDesk> noSqlDesks = new List<NoSqlDesk>();

            foreach (var desk in sqlDesks)
            {
                noSqlDesks.Add(new NoSqlDesk
                {
                    DeskName = desk.Name,
                    IsActive = desk.IsActive,
                    RoomName = desk.Room.Name,
                    FloorLevel = desk.Room.Floor.LevelNumber,
                    LocationName = desk.Room.Floor.Location.Name,
                    AssignedEquipments = desk.DeskEquipments.Select(de => de.Equipment.Name).ToList()
                });
            }

            if (noSqlDesks.Any())
                desksCollection.InsertMany(noSqlDesks);


            List<Reservation> sqlReservations = context.Reservations.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Desk)
                    .ThenInclude(d => d.Room)
                .ToList();

            List<NoSqlReservation> noSqlReservations = new List<NoSqlReservation>();

            foreach (var reservation in sqlReservations)
            {
                noSqlReservations.Add(new NoSqlReservation
                {
                    StartTime = reservation.StartTime,
                    EndTime = reservation.EndTime,
                    UserFullName = $"{reservation.User.FirstName} {reservation.User.LastName}",
                    UserEmail = reservation.User.Email,
                    DeskName = reservation.Desk.Name,
                    RoomName = reservation.Desk.Room.Name
                });
            }

            if (noSqlReservations.Any())
                reservationsCollection.InsertMany(noSqlReservations);


            List<Issue> sqlIssues = context.Issues.AsNoTracking()
                .Include(i => i.Desk)
                .Include(i => i.User)
                .ToList();

            List<NoSqlIssue> noSqlIssues = new List<NoSqlIssue>();

            foreach (var issue in sqlIssues)
            {
                noSqlIssues.Add(new NoSqlIssue
                {
                    Description = issue.Description,
                    ReportedAt = issue.ReportedAt,
                    IsResolved = issue.IsResolved,
                    ReportedBy = $"{issue.User.FirstName} {issue.User.LastName}",
                    DeskName = issue.Desk.Name
                });
            }

            if (noSqlIssues.Any())
                issuesCollection.InsertMany(noSqlIssues);

            Console.WriteLine("[SUCCESS] NoSQl database generated and saved to MongoDB successfully.");
        }
    }
}