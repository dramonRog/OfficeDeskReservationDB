using Microsoft.EntityFrameworkCore;
using OfficeDeskReservationDB.Data;
using OfficeDeskReservationDB.Models;

namespace OfficeDeskReservationSystemUI
{
    public class ItemUiWrapper
    {
        public int DisplayIndex { get; set; }
        public DisplayData Data { get; set; }
    }

    public class DisplayData
    {
        public string FirstName { get; set; }
        public string Email { get; set; }
        public object OriginalObject { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private readonly AppDbContext _context;
        private int _currentPage = 0;
        private const int _pageSize = 8;
        private string _selectedCategory = "Users";
        private DisplayData _itemBeingEdited = null;

        public MainPage(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            if (Application.Current != null) Application.Current.UserAppTheme = AppTheme.Light;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        // Nawigacja do strony administracji bazą
        private async void OnDatabaseAdminClicked(object sender, EventArgs e)
        {
            CustomDropdown.IsVisible = false;
            await Navigation.PushAsync(new DatabaseAdminPage(_context));
        }

        public async Task LoadData()
        {
            try
            {
                // FIX: Sprawdzenie czy baza istnieje przed zapytaniem
                if (!await _context.Database.CanConnectAsync())
                {
                    MainThread.BeginInvokeOnMainThread(() => {
                        BindableLayout.SetItemsSource(DataListContainer, null);
                        PageIndicator.Text = "0";
                    });
                    return;
                }

                List<ItemUiWrapper> wrappedList = new();
                if (_selectedCategory == "Users")
                {
                    var data = await _context.Users.AsNoTracking().OrderBy(u => u.Id).Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((u, i) => new ItemUiWrapper { DisplayIndex = (_currentPage * _pageSize) + i + 1, Data = new DisplayData { FirstName = $"{u.FirstName} {u.LastName}", Email = u.Email, OriginalObject = u } }).ToList();
                }
                else if (_selectedCategory == "Desks")
                {
                    var data = await _context.Desks.AsNoTracking().OrderBy(d => d.Id).Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((d, i) => new ItemUiWrapper { DisplayIndex = (_currentPage * _pageSize) + i + 1, Data = new DisplayData { FirstName = d.Name, Email = d.IsActive ? "Status: Active" : "Status: Inactive", OriginalObject = d } }).ToList();
                }
                else if (_selectedCategory == "Reservations")
                {
                    var data = await _context.Reservations.AsNoTracking().Include(r => r.Desk).OrderByDescending(r => r.StartTime).Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((r, i) => new ItemUiWrapper { DisplayIndex = (_currentPage * _pageSize) + i + 1, Data = new DisplayData { FirstName = $"Desk: {r.Desk?.Name ?? "N/A"}", Email = $"{r.StartTime:g} - {r.Status}", OriginalObject = r } }).ToList();
                }
                else if (_selectedCategory == "Issues")
                {
                    var data = await _context.Issues.AsNoTracking().OrderByDescending(i => i.ReportedAt).Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((issue, i) => new ItemUiWrapper { DisplayIndex = (_currentPage * _pageSize) + i + 1, Data = new DisplayData { FirstName = issue.Description, Email = issue.IsResolved ? "Resolved" : "Pending", OriginalObject = issue } }).ToList();
                }

                MainThread.BeginInvokeOnMainThread(() => {
                    BindableLayout.SetItemsSource(DataListContainer, wrappedList);
                    PageIndicator.Text = (_currentPage + 1).ToString();
                });
            }
            catch (Exception)
            {
                // W razie błędu tabel (np. baza istnieje ale jest pusta/uszkodzona) czyścimy listę
                MainThread.BeginInvokeOnMainThread(() => BindableLayout.SetItemsSource(DataListContainer, null));
            }
        }

        // --- Logika przycisków i modala (bez zmian, dla kompletności pliku) ---
        public void OnAddClicked(object sender, EventArgs e)
        {
            _itemBeingEdited = null;
            ModalTitle.Text = $"Add New {_selectedCategory.TrimEnd('s')}";
            EditEntry1.Text = string.Empty; EditEntry2.Text = string.Empty; EditEntry3.Text = string.Empty;
            EditBooleanSwitch.IsToggled = true;
            ConfigureModalForCategory();
            EditModalOverlay.IsVisible = true;
        }

        public void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is DisplayData d)
            {
                _itemBeingEdited = d;
                ModalTitle.Text = $"Edit {_selectedCategory.TrimEnd('s')}";
                ConfigureModalForCategory();
                if (d.OriginalObject is User u) { EditEntry1.Text = u.FirstName; EditEntry2.Text = u.LastName; EditEntry3.Text = u.Email; }
                else if (d.OriginalObject is Desk desk) { EditEntry1.Text = desk.Name; EditBooleanSwitch.IsToggled = desk.IsActive; }
                else if (d.OriginalObject is Reservation res) { EditEntry1.Text = res.Status; EditEntry2.Text = res.StartTime.ToString("g"); }
                else if (d.OriginalObject is Issue issue) { EditEntry1.Text = issue.Description; EditBooleanSwitch.IsToggled = issue.IsResolved; }
                EditModalOverlay.IsVisible = true;
            }
        }

        private void ConfigureModalForCategory()
        {
            EditContainer2.IsVisible = true; EditContainer3.IsVisible = false; EditBooleanContainer.IsVisible = false;
            if (_selectedCategory == "Users") { EditContainer3.IsVisible = true; EditLabel1.Text = "First Name"; EditLabel2.Text = "Last Name"; EditLabel3.Text = "Email"; }
            else if (_selectedCategory == "Desks") { EditContainer2.IsVisible = false; EditBooleanContainer.IsVisible = true; EditLabel1.Text = "Desk Name"; EditBooleanLabel.Text = "Is Active?"; }
            else if (_selectedCategory == "Reservations") { EditLabel1.Text = "Status"; EditLabel2.Text = "Start Time (yyyy-MM-dd HH:mm)"; }
            else if (_selectedCategory == "Issues") { EditContainer2.IsVisible = false; EditBooleanContainer.IsVisible = true; EditLabel1.Text = "Description"; EditBooleanLabel.Text = "Resolved?"; }
        }

        public async void OnSaveEditClicked(object sender, EventArgs e)
        {
            try
            {
                if (_itemBeingEdited == null)
                {
                    if (_selectedCategory == "Users") _context.Users.Add(new User { FirstName = EditEntry1.Text, LastName = EditEntry2.Text, Email = EditEntry3.Text, PasswordHash = "hashed", RoleId = 1, DepartmentId = 1 });
                    else if (_selectedCategory == "Desks") _context.Desks.Add(new Desk { Name = EditEntry1.Text, IsActive = EditBooleanSwitch.IsToggled, RoomId = 1 });
                    else if (_selectedCategory == "Reservations") { var u = await _context.Users.FirstOrDefaultAsync(); var d = await _context.Desks.FirstOrDefaultAsync(); _context.Reservations.Add(new Reservation { Status = EditEntry1.Text, StartTime = DateTime.TryParse(EditEntry2.Text, out DateTime dt) ? dt : DateTime.Now, UserId = u?.Id ?? 1, DeskId = d?.Id ?? 1 }); }
                    else if (_selectedCategory == "Issues") { var u = await _context.Users.FirstOrDefaultAsync(); var d = await _context.Desks.FirstOrDefaultAsync(); _context.Issues.Add(new Issue { Description = EditEntry1.Text, IsResolved = EditBooleanSwitch.IsToggled, ReportedAt = DateTime.Now, UserId = u?.Id ?? 1, DeskId = d?.Id ?? 1 }); }
                }
                else
                {
                    if (_itemBeingEdited.OriginalObject is User u) { var db = await _context.Users.FindAsync(u.Id); if (db != null) { db.FirstName = EditEntry1.Text; db.LastName = EditEntry2.Text; db.Email = EditEntry3.Text; } }
                    else if (_itemBeingEdited.OriginalObject is Desk d) { var db = await _context.Desks.FindAsync(d.Id); if (db != null) { db.Name = EditEntry1.Text; db.IsActive = EditBooleanSwitch.IsToggled; } }
                    else if (_itemBeingEdited.OriginalObject is Reservation r) { var db = await _context.Reservations.FindAsync(r.Id); if (db != null) { db.Status = EditEntry1.Text; if (DateTime.TryParse(EditEntry2.Text, out DateTime dt)) db.StartTime = dt; } }
                    else if (_itemBeingEdited.OriginalObject is Issue i) { var db = await _context.Issues.FindAsync(i.Id); if (db != null) { db.Description = EditEntry1.Text; db.IsResolved = EditBooleanSwitch.IsToggled; } }
                }
                await _context.SaveChangesAsync();
                EditModalOverlay.IsVisible = false;
                await LoadData();
            }
            catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
        }

        public void OnCancelEditClicked(object sender, EventArgs e) => EditModalOverlay.IsVisible = false;

        public async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is DisplayData d)
            {
                if (await DisplayAlert("Delete", "Are you sure?", "Yes", "No"))
                {
                    _context.Remove(d.OriginalObject); await _context.SaveChangesAsync(); await LoadData();
                }
            }
        }

        private async void OnHeaderClicked(object sender, EventArgs e) { CustomDropdown.IsVisible = !CustomDropdown.IsVisible; if (CustomDropdown.IsVisible) { CustomDropdown.Opacity = 0; await CustomDropdown.FadeTo(1, 150); } }
        private async void OnMenuItemClicked(object sender, EventArgs e) { if (sender is Button btn) { _selectedCategory = btn.CommandParameter.ToString(); CurrentCategoryLabel.Text = _selectedCategory; CustomDropdown.IsVisible = false; _currentPage = 0; await LoadData(); } }
        public async void OnPreviousClicked(object sender, EventArgs e) { if (_currentPage > 0) { _currentPage--; await LoadData(); } }
        public async void OnNextClicked(object sender, EventArgs e) { _currentPage++; await LoadData(); }
        private void OnScrollViewScrolled(object sender, ScrolledEventArgs e) { }
    }
}