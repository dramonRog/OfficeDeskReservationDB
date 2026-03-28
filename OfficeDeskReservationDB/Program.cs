using OfficeDeskReservationDB.Data;
using System;
using System.IO;

namespace OfficeDataGenerator
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new AppDbContext())
            {
                Console.WriteLine("Initializing the database...");
                context.Database.EnsureCreated();

                bool isRunning = true;
                string workingDirectory = AppContext.BaseDirectory;

                string projectRootFolder = Path.GetFullPath(Path.Combine(workingDirectory, @"..\..\..\"));

                string backupPath = Path.Combine(projectRootFolder, "database_backup.json");
                string testPath = Path.Combine(projectRootFolder, "test_import.json");

                while (isRunning)
                {
                    Console.Clear();
                    Console.WriteLine("=== OFFICE DESK RESERVATION SYSTEM ===");
                    Console.WriteLine("1. Generate random data (non-dictionary tables)");
                    Console.WriteLine("2. Export database to JSON");
                    Console.WriteLine("3. Import database from JSON");
                    Console.WriteLine("4. Delete entire database (Drop)");
                    Console.WriteLine("5. Create database (EnsureCreated)");
                    Console.WriteLine("6. Exit");
                    Console.WriteLine("======================================");
                    Console.Write("Select an option (1-6): ");

                    string? choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            if (!context.Database.CanConnect())
                            {
                                Console.WriteLine("\n[ERROR] Database does not exist! Please create it first (Option 5).");
                                break;
                            }

                            Console.Write("\nEnter the count of generating random data (10, 100, 1000...): ");
                            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
                            {
                                Console.WriteLine("[ERROR] Invalid number!!!");
                            }
                            else
                            {
                                Generator dataGenerator = new Generator(context);
                                dataGenerator.GenerateDesks(count, context);
                                dataGenerator.GenerateDeskEquipments(count, context);
                                dataGenerator.GenerateUsers(count, context);
                                dataGenerator.GenerateIssues(count, context);
                                dataGenerator.GenerateReservations(count, context);
                                Console.WriteLine("\n[SUCCESS] Random data generation completed.");
                            }
                            break;

                        case "2":
                            if (!context.Database.CanConnect())
                            {
                                Console.WriteLine("\n[ERROR] Database does not exist! Please create it first (Option 5).");
                                break;
                            }

                            DataTransfer.ExportDatabaseToJson(context, backupPath);
                            break;

                        case "3":
                            if (!context.Database.CanConnect())
                            {
                                Console.WriteLine("\n[ERROR] Database does not exist! Please create it first (Option 5).");
                                break;
                            }

                            DataTransfer.ImportDatabaseFromJson(context, testPath);
                            break;

                        case "4":
                            Console.WriteLine("\n[WARNING] Deleting the database...");
                            bool isDeleted = context.Database.EnsureDeleted();
                            if (isDeleted)
                            {
                                Console.WriteLine("[SUCCESS] Database successfully deleted.");
                            }
                            else
                            {
                                Console.WriteLine("[INFO] Database did not exist, nothing to delete.");
                            }
                            break;

                        case "5":
                            Console.WriteLine("\n[INFO] Creating the database...");
                            bool isCreated = context.Database.EnsureCreated();
                            if (isCreated)
                            {
                                Console.WriteLine("[SUCCESS] Database successfully created and dictionary tables populated.");
                            }
                            else
                            {
                                Console.WriteLine("[INFO] Database already exists. No need to recreate.");
                            }
                            break;

                        case "6":
                            Console.WriteLine("\nExiting the program. Goodbye!");
                            isRunning = false;
                            break;

                        default:
                            Console.WriteLine("\n[ERROR] Invalid option! Please select a number between 1 and 6.");
                            break;
                    }

                    if (isRunning)
                    {
                        Console.WriteLine("\nPress any key to return to the main menu...");
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}