using OfficeDeskReservationDB.Data;

namespace OfficeDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();

                Console.WriteLine("Database and tables created successfully!");
            }
        }
    }
}