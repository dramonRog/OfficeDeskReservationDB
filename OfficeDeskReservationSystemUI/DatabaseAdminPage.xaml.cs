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
        // OPERACJA: USUWANIE BAZY DANYCH
        // ==========================================
        private async void OnDeleteDatabaseClicked(object sender, EventArgs e)
        {
            try
            {
                // Sprawdzenie, czy połączenie z bazą jest możliwe (czy plik istnieje)
                bool exists = await _context.Database.CanConnectAsync();

                if (!exists)
                {
                    await DisplayAlert("Info", "Database does not exist. There is nothing to delete.", "OK");
                    return;
                }

                // Potwierdzenie krytycznej operacji
                bool confirm = await DisplayAlert("CRITICAL ACTION",
                    "Are you absolutely sure you want to DELETE the entire database? This action is irreversible.",
                    "YES, DELETE", "CANCEL");

                if (confirm)
                {
                    // Fizyczne usunięcie bazy danych
                    bool deleted = await _context.Database.EnsureDeletedAsync();

                    if (deleted)
                    {
                        await DisplayAlert("Success", "Database has been deleted successfully.", "OK");
                        // Powrót do strony głównej po usunięciu
                        await Navigation.PopAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}