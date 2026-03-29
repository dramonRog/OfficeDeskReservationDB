using Microsoft.EntityFrameworkCore;
using OfficeDeskReservationDB.Data;

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

        private double _startScrollY;
        private bool _isMenuAnimating = false;

        public MainPage(AppDbContext context)
        {
            InitializeComponent();
            _context = context;

            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async void OnHeaderClicked(object sender, EventArgs e)
        {
            if (_isMenuAnimating) return;

            if (!CustomDropdown.IsVisible)
            {
                _isMenuAnimating = true;
                CustomDropdown.InputTransparent = false;
                CustomDropdown.TranslationY = -15;
                CustomDropdown.Opacity = 0;
                CustomDropdown.IsVisible = true;

                await Task.WhenAll(
                    CustomDropdown.FadeTo(1, 150, Easing.CubicOut),
                    CustomDropdown.TranslateTo(0, 0, 150, Easing.CubicOut)
                );
                _isMenuAnimating = false;
            }
            else
            {
                _isMenuAnimating = true;
                CustomDropdown.InputTransparent = true;

                await Task.WhenAll(
                    CustomDropdown.FadeTo(0, 150, Easing.CubicIn),
                    CustomDropdown.TranslateTo(0, -15, 150, Easing.CubicIn)
                );

                CustomDropdown.IsVisible = false;
                _isMenuAnimating = false;
            }
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            if (_isMenuAnimating) return;
            _isMenuAnimating = true;

            CustomDropdown.InputTransparent = true;

            if (sender is Button button)
            {
                await Task.WhenAll(
                    button.TranslateTo(15, 0, 100, Easing.CubicOut),
                    button.FadeTo(0.5, 100, Easing.CubicOut)
                );
                await Task.WhenAll(
                    button.TranslateTo(0, 0, 100, Easing.CubicIn),
                    button.FadeTo(1.0, 100, Easing.CubicIn)
                );

                _selectedCategory = button.CommandParameter.ToString();
                CurrentCategoryLabel.Text = _selectedCategory;

                await Task.WhenAll(
                    CustomDropdown.FadeTo(0, 150, Easing.CubicIn),
                    CustomDropdown.TranslateTo(0, -15, 150, Easing.CubicIn)
                );

                CustomDropdown.IsVisible = false;

                _currentPage = 0;
                await LoadData();
            }

            _isMenuAnimating = false;
        }

        public async Task LoadData()
        {
            try
            {
                List<ItemUiWrapper> wrappedList = new();

                if (_selectedCategory == "Users")
                {
                    var data = await _context.Users.AsNoTracking().OrderBy(u => u.Id)
                        .Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((u, i) => new ItemUiWrapper
                    {
                        DisplayIndex = (_currentPage * _pageSize) + i + 1,
                        Data = new DisplayData { FirstName = $"{u.FirstName} {u.LastName}", Email = u.Email, OriginalObject = u }
                    }).ToList();
                }
                else if (_selectedCategory == "Desks")
                {
                    var data = await _context.Desks.AsNoTracking().OrderBy(d => d.Id)
                        .Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((d, i) => new ItemUiWrapper
                    {
                        DisplayIndex = (_currentPage * _pageSize) + i + 1,
                        Data = new DisplayData { FirstName = d.Name, Email = d.IsActive ? "Status: Active" : "Status: Inactive", OriginalObject = d }
                    }).ToList();
                }
                else if (_selectedCategory == "Reservations")
                {
                    var data = await _context.Reservations.AsNoTracking().Include(r => r.Desk)
                        .OrderByDescending(r => r.StartTime).Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((r, i) => new ItemUiWrapper
                    {
                        DisplayIndex = (_currentPage * _pageSize) + i + 1,
                        Data = new DisplayData { FirstName = $"Desk: {r.Desk?.Name}", Email = $"{r.StartTime:g} - {r.Status}", OriginalObject = r }
                    }).ToList();
                }
                else if (_selectedCategory == "Issues")
                {
                    var data = await _context.Issues.AsNoTracking().OrderByDescending(i => i.ReportedAt)
                        .Skip(_currentPage * _pageSize).Take(_pageSize).ToListAsync();
                    wrappedList = data.Select((issue, i) => new ItemUiWrapper
                    {
                        DisplayIndex = (_currentPage * _pageSize) + i + 1,
                        Data = new DisplayData { FirstName = issue.Description, Email = $"Reported: {issue.ReportedAt:d}", OriginalObject = issue }
                    }).ToList();
                }

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    BindableLayout.SetItemsSource(DataListContainer, wrappedList);
                    PageIndicator.Text = (_currentPage + 1).ToString();

                    await Task.Delay(50);
                    UpdateCustomScrollbar();
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void OnScrollViewScrolled(object sender, ScrolledEventArgs e)
        {
            UpdateCustomScrollbar();
        }

        private void UpdateCustomScrollbar()
        {
            if (DataScrollView.ContentSize.Height <= 0) return;

            double maxScroll = DataScrollView.ContentSize.Height - DataScrollView.Height;
            if (maxScroll <= 0) { ScrollTrack.IsVisible = false; return; }

            ScrollTrack.IsVisible = true;
            double viewportRatio = DataScrollView.Height / DataScrollView.ContentSize.Height;
            double thumbHeight = Math.Max(30, ScrollTrack.Height * viewportRatio);
            ScrollThumb.HeightRequest = thumbHeight;

            double currentScrollRatio = DataScrollView.ScrollY / maxScroll;
            currentScrollRatio = Math.Max(0, Math.Min(1, currentScrollRatio));

            ScrollThumb.TranslationY = currentScrollRatio * (ScrollTrack.Height - thumbHeight);
        }

        private void OnScrollThumbPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (DataScrollView.ContentSize.Height <= 0) return;

            double maxScroll = DataScrollView.ContentSize.Height - DataScrollView.Height;
            if (maxScroll <= 0) return;

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _startScrollY = DataScrollView.ScrollY;
                    break;
                case GestureStatus.Running:
                    double thumbTravel = ScrollTrack.Height - ScrollThumb.Height;
                    if (thumbTravel <= 0) return;

                    double deltaRatio = e.TotalY / thumbTravel;
                    double newScrollY = _startScrollY + (deltaRatio * maxScroll);
                    newScrollY = Math.Max(0, Math.Min(maxScroll, newScrollY));

                    DataScrollView.ScrollToAsync(0, newScrollY, false);
                    break;
            }
        }

        public async void OnPreviousClicked(object sender, EventArgs e) { if (_currentPage > 0) { _currentPage--; await LoadData(); } }
        public async void OnNextClicked(object sender, EventArgs e) { _currentPage++; await LoadData(); }

        public async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is DisplayData d)
                await DisplayAlert("Edit", $"Editing: {d.FirstName}", "OK");
        }

        public async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is DisplayData d)
            {
                if (await DisplayAlert("Delete", "Are you sure you want to delete this item?", "Yes", "No"))
                {
                    try
                    {
                        _context.Remove(d.OriginalObject);
                        await _context.SaveChangesAsync();

                        await LoadData();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                }
            }
        }
    }
}