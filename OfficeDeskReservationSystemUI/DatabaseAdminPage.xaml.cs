using System.Reflection;
using System.Text;
using MongoDB.Driver;
using OfficeDeskReservationDB.Data;
using OfficeDeskReservationDB.Testing;

namespace OfficeDeskReservationSystemUI
{
    public partial class DatabaseAdminPage : ContentPage
    {
        private readonly AppDbContext _context;

        public DatabaseAdminPage(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private async void OnBackClicked(object sender, EventArgs e) => await Navigation.PopAsync();

        private void ShowLoading(string message)
        {
            LoadingMessageLabel.Text = message;
            LoadingOverlay.IsVisible = true;
            LoadingIndicator.IsRunning = true;
        }

        private void HideLoading()
        {
            LoadingOverlay.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }

        private async void OnCreateDatabaseClicked(object sender, EventArgs e)
        {
            try
            {
                ShowLoading("Initializing database...");
                await Task.Delay(100);

                bool created = await _context.Database.EnsureCreatedAsync();

                HideLoading();
                await DisplayAlert(created ? "Success" : "Info", created ? "Empty database and tables created successfully." : "Database already exists.", "OK");
                if (created) await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                HideLoading();
                await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
            }
        }

        private async void OnGenerateDataClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(GenCountEntry.Text, out int count) || count <= 0)
            {
                await DisplayAlert("Invalid Input", "Please enter a valid positive number.", "OK");
                return;
            }

            try
            {
                ShowLoading($"Generating {count} records...");
                await Task.Run(() => {
                    var gen = new Generator(_context);
                    gen.GenerateUsers(count);
                    gen.GenerateDesks(count);
                    gen.GenerateReservations(count);
                    gen.GenerateIssues(count);
                });

                HideLoading();
                await DisplayAlert("Success", $"Successfully generated {count} records per category.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                HideLoading();
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            try
            {
                ShowLoading("Exporting database to JSON...");

                string binDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(binDir).Parent.Parent.Parent.FullName;
                string path = Path.Combine(projectRoot, "database_backup.json");

                await Task.Run(() => DataTransfer.ExportDatabaseToJson(_context, path));

                HideLoading();
                await DisplayAlert("Export Success", $"File saved to project root:\n{path}", "OK");
            }
            catch (Exception ex)
            {
                HideLoading();
                await DisplayAlert("Export Error", ex.Message, "OK");
            }
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select JSON Database File" });
                if (result != null)
                {
                    ShowLoading("Importing data from JSON...");
                    string summary = await Task.Run(() => DataTransfer.ImportDatabaseFromJson(_context, result.FullPath));

                    HideLoading();
                    await DisplayAlert("Import Result", summary, "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                HideLoading();
                await DisplayAlert("Import Error", ex.Message, "OK");
            }
        }

        private async void OnTransformToNoSqlClicked(object sender, EventArgs e)
        {
            try
            {
                ShowLoading("Checking SQL Database connection...");
                await Task.Delay(100);

                if (!await _context.Database.CanConnectAsync())
                {
                    HideLoading();
                    await DisplayAlert("Error", "SQL Database not found. Create it and add data first.", "OK");
                    return;
                }

                ShowLoading("Migrating data to MongoDB...");
                await Task.Run(() => DataTransfer.TransformToNoSql(_context));

                HideLoading();
                await DisplayAlert("NoSQL Success", "Data denormalized and migrated to local MongoDB instance successfully.", "OK");
            }
            catch (Exception ex)
            {
                HideLoading();
                await DisplayAlert("NoSQL Error", $"Failed to transform: {ex.Message}", "OK");
            }
        }

        // ==========================================
        // OPERACJA: BENCHMARK / TESTY WYDAJNOŚCI
        // ==========================================
        private async void OnRunTestsClicked(object sender, EventArgs e)
        {
            try
            {
                ShowLoading("Running Performance Benchmarks...");
                string benchmarkResult = "";

                await Task.Run(() =>
                {
                    // Zapamiętujemy oryginalne wyjście konsoli
                    var originalConsoleOut = Console.Out;

                    try
                    {
                        // Tworzymy strumień tekstowy, by przechwycić to, co metody z biblioteki wrzucają do Console.WriteLine
                        using (var stringWriter = new StringWriter())
                        {
                            Console.SetOut(stringWriter);

                            var client = new MongoClient("mongodb://localhost:27017");
                            var database = client.GetDatabase("OfficeDeskReservationDB");

                            // Używamy REFEKSJI, aby wywołać prywatne metody z Twojej statycznej klasy PerformanceBenchmarker
                            var type = typeof(PerformanceBenchmarker);
                            var flags = BindingFlags.NonPublic | BindingFlags.Static;

                            var m1 = type.GetMethod("TestDeepFetch", flags);
                            var m2 = type.GetMethod("TestSearchByEmail", flags);
                            var m3 = type.GetMethod("TestMassUpdate", flags);

                            // Bezpiecznie odpalamy po kolei testy z oryginalnej biblioteki
                            m1?.Invoke(null, new object[] { _context, database });
                            m2?.Invoke(null, new object[] { _context, database });
                            m3?.Invoke(null, new object[] { _context, database });

                            // Zgrywamy cały przechwycony tekst
                            benchmarkResult = stringWriter.ToString();
                        }
                    }
                    finally
                    {
                        // Przywracamy domyślną konsolę (wymagane, by nie zepsuć reszty aplikacji)
                        Console.SetOut(originalConsoleOut);
                    }
                });

                HideLoading();

                if (string.IsNullOrWhiteSpace(benchmarkResult))
                    benchmarkResult = "Benchmark finished, but no output was generated.";

                await DisplayAlert("Benchmark Results", benchmarkResult, "OK");
            }
            catch (Exception ex)
            {
                HideLoading();
                await DisplayAlert("Test Error", $"Failed to run benchmark: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteDatabaseClicked(object sender, EventArgs e)
        {
            if (!await _context.Database.CanConnectAsync())
            {
                await DisplayAlert("Info", "Database does not exist.", "OK");
                return;
            }

            if (await DisplayAlert("CRITICAL ACTION", "Delete entire database? This is irreversible.", "YES, DELETE", "CANCEL"))
            {
                try
                {
                    ShowLoading("Deleting database...");
                    await Task.Delay(100);

                    await _context.Database.EnsureDeletedAsync();

                    HideLoading();
                    await DisplayAlert("Success", "Database wiped successfully.", "OK");
                    await Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    HideLoading();
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }
    }
}