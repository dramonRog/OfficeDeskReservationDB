using Microsoft.EntityFrameworkCore;
using OfficeDeskReservationDB.Data;
using UraniumUI;

namespace OfficeDeskReservationSystemUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseUraniumUI()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFontAwesomeIconFonts();
                });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer("Server=localhost;Database=OfficeDeskReservationDBSystem;Trusted_Connection=True;TrustServerCertificate=True;",
                sqlOptions => sqlOptions.CommandTimeout(15)));

            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}