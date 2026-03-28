using OfficeDeskReservationDB.Data;

namespace OfficeDataGenerator
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new AppDbContext())
            {
                Console.WriteLine("Inicjalizacja bazy danych...");

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
                            DataTransfer.ExportDatabaseToJson(context, backupPath);
                            break;

                        case "3":
                            DataTransfer.ImportDatabaseFromJson(context, testPath);
                            break;

                        case "4":
                            Console.WriteLine("\n[WARNING] Trwa usuwanie bazy danych...");
                            bool isDeleted = context.Database.EnsureDeleted();
                            if (isDeleted)
                            {
                                Console.WriteLine("[SUCCESS] Baza danych została całkowicie usunięta.");
                            }
                            else
                            {
                                Console.WriteLine("[INFO] Baza danych nie istniała, więc nie było co usuwać.");
                            }
                            break;

                        case "5":
                            Console.WriteLine("\n[INFO] Trwa tworzenie bazy danych...");
                            bool isCreated = context.Database.EnsureCreated();
                            if (isCreated)
                            {
                                Console.WriteLine("[SUCCESS] Baza danych została utworzona, a tabele słownikowe wypełnione.");
                            }
                            else
                            {
                                Console.WriteLine("[INFO] Baza danych już istnieje. Nie trzeba jej tworzyć ponownie.");
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