using OfficeDeskReservationDB.Data;

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

        // ==========================================
        // OPERACJA: UTWORZENIE PUSTEJ BAZY
        // ==========================================
        private async void OnCreateDatabaseClicked(object sender, EventArgs e)
        {
            try
            {
                bool created = await _context.Database.EnsureCreatedAsync();
                await DisplayAlert(created ? "Success" : "Info", created ? "Empty database and tables created successfully." : "Database already exists.", "OK");
                if (created) await Navigation.PopAsync();
            }
            catch (Exception ex) { await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK"); }
        }

        // ==========================================
        // OPERACJA: GENERATOR DANYCH
        // ==========================================
        private async void OnGenerateDataClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(GenCountEntry.Text, out int count) || count <= 0)
            {
                await DisplayAlert("Invalid Input", "Please enter a valid positive number.", "OK");
                return;
            }
            try
            {
                await Task.Run(() => {
                    var gen = new Generator(_context);
                    gen.GenerateUsers(count);
                    gen.GenerateDesks(count);
                    gen.GenerateReservations(count);
                    gen.GenerateIssues(count);
                });
                await DisplayAlert("Success", $"Successfully generated {count} records per category.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        // ==========================================
        // EXPORT: Zapisuje w root projektu
        // ==========================================
        private async void OnExportClicked(object sender, EventArgs e)
        {
            try
            {
                string binDir = AppDomain.CurrentDomain.BaseDirectory;
                string projectRoot = Directory.GetParent(binDir).Parent.Parent.Parent.FullName;
                string path = Path.Combine(projectRoot, "database_backup.json");

                await Task.Run(() => DataTransfer.ExportDatabaseToJson(_context, path));
                await DisplayAlert("Export Success", $"File saved to project root:\n{path}", "OK");
            }
            catch (Exception ex) { await DisplayAlert("Export Error", ex.Message, "OK"); }
        }

        // ==========================================
        // IMPORT: Z podsumowaniem dodanych rekordów
        // ==========================================
        private async void OnImportClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select JSON Database File" });
                if (result != null)
                {
                    string summary = await Task.Run(() => DataTransfer.ImportDatabaseFromJson(_context, result.FullPath));
                    await DisplayAlert("Import Result", summary, "OK");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex) { await DisplayAlert("Import Error", ex.Message, "OK"); }
        }

        // ==========================================
        // OPERACJA: TRANSFORMACJA NoSQL (MongoDB)
        // ==========================================
        private async void OnTransformToNoSqlClicked(object sender, EventArgs e)
        {
            try
            {
                // Sprawdzenie czy baza SQL jest dostępna
                if (!await _context.Database.CanConnectAsync())
                {
                    await DisplayAlert("Error", "SQL Database not found. Create it and add data first.", "OK");
                    return;
                }

                // Wywołanie gotowej metody transformacji z DataTransfer.cs
                await Task.Run(() => DataTransfer.TransformToNoSql(_context));

                await DisplayAlert("NoSQL Success", "Data denormalized and migrated to local MongoDB instance successfully.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("NoSQL Error", $"Failed to transform: {ex.Message}", "OK");
            }
        }

        // ==========================================
        // OPERACJA: USUWANIE BAZY
        // ==========================================
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
                    await _context.Database.EnsureDeletedAsync();
                    await DisplayAlert("Success", "Database wiped successfully.", "OK");
                    await Navigation.PopAsync();
                }
                catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
            }
        }
    }
}