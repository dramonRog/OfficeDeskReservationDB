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