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

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        // ==========================================
        // OPERACJA: UTWORZENIE PUSTEJ BAZY
        // ==========================================
        private async void OnCreateDatabaseClicked(object sender, EventArgs e)
        {
            try
            {
                bool created = await _context.Database.EnsureCreatedAsync();
                if (created)
                {
                    await DisplayAlert("Success", "Empty database and tables created successfully.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Info", "Database already exists.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
            }
        }

        // ==========================================
        // OPERACJA: GENERATOR DANYCH
        // ==========================================
        private async void OnGenerateDataClicked(object sender, EventArgs e)
        {
            // 1. Walidacja wejścia
            if (!int.TryParse(GenCountEntry.Text, out int count) || count <= 0)
            {
                await DisplayAlert("Invalid Input", "Please enter a valid positive number.", "OK");
                return;
            }

            if (!await _context.Database.CanConnectAsync())
            {
                await DisplayAlert("Error", "Database does not exist. Create it first.", "OK");
                return;
            }

            try
            {
                await Task.Run(() =>
                {
                    var generator = new Generator(_context);

                    generator.GenerateUsers(count);
                    generator.GenerateDesks(count);
                    generator.GenerateReservations(count);
                    generator.GenerateIssues(count);
                });

                await DisplayAlert("Success", $"Successfully generated {count} records for each category.", "OK");
                await Navigation.PopAsync(); 
            }
            catch (Exception ex)
            {
                await DisplayAlert("Generation Error", $"Failed: {ex.Message}", "OK");
            }
        }

        // ==========================================
        // OPERACJA: USUWANIE BAZY DANYCH
        // ==========================================
        private async void OnDeleteDatabaseClicked(object sender, EventArgs e)
        {
            try
            {
                bool exists = await _context.Database.CanConnectAsync();

                if (!exists)
                {
                    await DisplayAlert("Info", "Database does not exist.", "OK");
                    return;
                }

                bool confirm = await DisplayAlert("CRITICAL ACTION",
                    "Are you absolutely sure you want to DELETE the entire database?",
                    "YES, DELETE", "CANCEL");

                if (confirm)
                {
                    bool deleted = await _context.Database.EnsureDeletedAsync();
                    if (deleted)
                    {
                        await DisplayAlert("Success", "Database has been deleted.", "OK");
                        await Navigation.PopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}