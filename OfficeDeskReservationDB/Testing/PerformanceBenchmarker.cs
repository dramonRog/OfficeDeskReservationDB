using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using OfficeDeskReservationDB.Data;
using OfficeDeskReservationDB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OfficeDeskReservationDB.Testing
{
    public static class PerformanceBenchmarker
    {
        public static void RunPerformanceMenu(AppDbContext sqlContext)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("OfficeDeskReservationDB");

            bool backToMain = false;
            while (!backToMain)
            {
                Console.Clear();
                Console.WriteLine("=== DATABASE PERFORMANCE TEST CENTER ===");
                Console.WriteLine("1. [READ] Deep Fetch (Reservations + JOINs)");
                Console.WriteLine("2. [SEARCH] Find User by Email (Indexing test)");
                Console.WriteLine("3. [UPDATE] Mass Update Department Name (Consistency test)");
                Console.WriteLine("4. Return to Main Menu");
                Console.WriteLine("========================================");
                Console.Write("Select test case: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1": TestDeepFetch(sqlContext, database); break;
                    case "2": TestSearchByEmail(sqlContext, database); break;
                    case "3": TestMassUpdate(sqlContext, database); break;
                    case "4": backToMain = true; break;
                }
                if (!backToMain) { Console.WriteLine("\nPress any key..."); Console.ReadKey(); }
            }
        }

        private static void TestDeepFetch(AppDbContext sql, IMongoDatabase nosql)
        {
            Console.WriteLine("\n--- TEST 1: DEEP FETCH (JOINs vs FLAT) ---");
            Stopwatch sw = new Stopwatch();

            // SQL
            sw.Start();
            var sqlData = sql.Reservations.AsNoTracking()
                .Include(r => r.User).Include(r => r.Desk).ThenInclude(d => d.Room)
                .ToList();
            sw.Stop();
            Console.WriteLine($"SQL Server (JOINs): {sw.ElapsedMilliseconds} ms");

            // NoSQL
            var collection = nosql.GetCollection<NoSqlReservation>("Reservations");
            sw.Restart();
            var nosqlData = collection.Find(_ => true).ToList();
            sw.Stop();
            Console.WriteLine($"MongoDB (Flat): {sw.ElapsedMilliseconds} ms");
        }

        private static void TestSearchByEmail(AppDbContext sql, IMongoDatabase nosql)
        {
            string emailToFind = "test_user_50000@example.com"; 
            Console.WriteLine($"\n--- TEST 2: SEARCH BY EMAIL ({emailToFind}) ---");
            Stopwatch sw = new Stopwatch();

            // SQL
            sw.Start();
            var sqlUser = sql.Users.AsNoTracking().FirstOrDefault(u => u.Email == emailToFind);
            sw.Stop();
            Console.WriteLine($"SQL Search: {sw.ElapsedMilliseconds} ms");

            // NoSQL
            var collection = nosql.GetCollection<NoSqlUser>("Users");
            sw.Restart();
            var nosqlUser = collection.Find(u => u.Email == emailToFind).FirstOrDefault();
            sw.Stop();
            Console.WriteLine($"MongoDB Search: {sw.ElapsedMilliseconds} ms");
        }

        private static void TestMassUpdate(AppDbContext sql, IMongoDatabase nosql)
        {
            Console.WriteLine("\n--- TEST 3: MASS UPDATE (Department Rename) ---");
            string oldName = "IT Department";
            string newName = "Global IT Solutions";
            Stopwatch sw = new Stopwatch();

            sw.Start();
            var dept = sql.Departments.FirstOrDefault(d => d.Name == oldName);
            if (dept != null) { dept.Name = newName; sql.SaveChanges(); }
            sw.Stop();
            Console.WriteLine($"SQL Update (Normalized): {sw.ElapsedMilliseconds} ms");

            var collection = nosql.GetCollection<NoSqlUser>("Users");
            sw.Restart();
            var filter = Builders<NoSqlUser>.Filter.Eq(u => u.DepartmentName, oldName);
            var update = Builders<NoSqlUser>.Update.Set(u => u.DepartmentName, newName);
            collection.UpdateMany(filter, update);
            sw.Stop();
            Console.WriteLine($"MongoDB Update (Denormalized): {sw.ElapsedMilliseconds} ms");
        }
    }
}
