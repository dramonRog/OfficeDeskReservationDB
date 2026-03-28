using OfficeDeskReservationDB.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace OfficeDeskReservationDB.Data
{
    public class DatabaseBackup
    {
        public List<Location> Locations { get; set; } = new();
        public List<Floor> Floors { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();
        public List<Equipment> Equipments { get; set; } = new();
        public List<Role> Roles { get; set; } = new();
        public List<Department> Departments { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<Desk> Desks { get; set; } = new();
        public List<DeskEquipment> DeskEquipments { get; set; } = new();
        public List<Reservation> Reservations { get; set; } = new();
        public List<Issue> Issues { get; set; } = new();
    }

    public class DataTransfer
    {
        private readonly AppDbContext _context;

        public DataTransfer(AppDbContext context)
        {
            _context = context;
        }

        public void ExportUsersToJson(string filePath)
        {
            Console.WriteLine("Downloading data from the database...");

            DatabaseBackup backup = new DatabaseBackup
            {
                Locations = _context.Locations.ToList(),
                Floors = _context.Floors.ToList(),
                Rooms = _context.Rooms.ToList(),
                Equipments = _context.Equipments.ToList(),
                Roles = _context.Roles.ToList(),
                Departments = _context.Departments.ToList(),
                Users = _context.Users.ToList(),
                Desks = _context.Desks.ToList(),
                DeskEquipments = _context.DeskEquipments.ToList(),
                Reservations = _context.Reservations.ToList(),
                Issues = _context.Issues.ToList()
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(backup, options);

            File.WriteAllText(filePath, jsonString);
            Console.WriteLine($"Success, database was exported into file: {filePath}");
        }

        public static void ImportDatabaseFromJson(AppDbContext context, string filePath)
        {
            Console.WriteLine("\nRozpoczynam import z pliku JSON i zapis do bazy...");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("[BŁĄD] Nie znaleziono pliku.");
                return;
            }

            string jsonString = File.ReadAllText(filePath);
            // System.Text.Json domyślnie ignoruje nieznane pola (np. Department_) - to chroni nasz program!
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

                if (backup.Reservations != null)
                {
                    foreach (var res in backup.Reservations)
                    {
                        if (!context.Reservations.Any(r => r.DeskId == res.DeskId && r.StartTime == res.StartTime))
                        {
                            res.Id = 0;
                            context.Reservations.Add(res);
                            addedRes++;
                        }
                    }
                }

                if (backup.Issues != null)
                {
                    foreach (var issue in backup.Issues)
                    {
                        if (!context.Issues.Any(i => i.Description == issue.Description && i.ReportedAt == issue.ReportedAt))
                        {
                            issue.Id = 0;
                            context.Issues.Add(issue);
                            addedIssues++;
                        }
                    }
                }

                if (backup.DeskEquipments != null)
                {
                    foreach (var de in backup.DeskEquipments)
                    {
                        if (!context.DeskEquipments.Any(x => x.DeskId == de.DeskId && x.EquipmentId == de.EquipmentId))
                        {
                            context.DeskEquipments.Add(de);
                            addedEq++;
                        }
                    }
                }

                context.SaveChanges();

                Console.WriteLine($"[SUKCES] Pominięto duplikaty. Zapisano nowe rekordy z JSON:");
                Console.WriteLine($"- {addedUsers} Użytkowników");
                Console.WriteLine($"- {addedDesks} Biurek");
                Console.WriteLine($"- {addedRes} Rezerwacji");
                Console.WriteLine($"- {addedIssues} Zgłoszeń");
                Console.WriteLine($"- {addedEq} Przypisań sprzętu");
            }
        }
    }
}
