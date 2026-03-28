using Bogus;
using Microsoft.EntityFrameworkCore.Storage;
using OfficeDeskReservationDB.Data;
using OfficeDeskReservationDB.Models;
using System.Security.Cryptography;
using System.Text;

namespace OfficeDataGenerator
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new AppDbContext())
            {
                context.Database.EnsureCreated();

                Console.WriteLine("Enter the count of generating random data for non-dictionary tables(10, 100, 1000, ...):");

                if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
                {
                    Console.WriteLine("Invalid number!!!");
                    return;
                }

                Generator dataGenerator = new Generator(context);

                dataGenerator.GenerateDesks(count, context);
                dataGenerator.GenerateDeskEquipments(count, context);
                dataGenerator.GenerateUsers(count, context);
                dataGenerator.GenerateIssues(count, context);
                dataGenerator.GenerateReservations(count, context);
            }

            Console.WriteLine("Enter any key to close the program...");
            Console.ReadKey();
        }
    }
}