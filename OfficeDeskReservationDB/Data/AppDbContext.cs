using Microsoft.EntityFrameworkCore;
using OfficeDeskReservationDB.Models;

namespace OfficeDeskReservationDB.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Desk> Desks { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<DeskEquipment> DeskEquipments { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Issue> Issues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeskEquipment>()
                .HasKey(de => new { de.DeskId, de.EquipmentId });

            modelBuilder.Entity<DeskEquipment>()
                .HasOne(de => de.Desk)
                .WithMany(d => d.DeskEquipments)
                .HasForeignKey(de => de.DeskId);

            modelBuilder.Entity<DeskEquipment>()
                .HasOne(de => de.Equipment)
                .WithMany(e => e.DeskEquipments)
                .HasForeignKey(de => de.EquipmentId);

            modelBuilder.Entity<Location>().HasData(
                new Location { Id = 1, Name = "LocA", Address = "Mikolajczyl", City = "Opole" },
                new Location { Id = 2, Name = "LocB", Address = "Proszkowska", City = "Opole" },
                new Location { Id = 3, Name = "LocC", Address = "Malopolska", City = "Opole" }
            );

            modelBuilder.Entity<Floor>().HasData(
                new Floor { Id = 1, LevelNumber = 1, LocationId = 1 },
                new Floor { Id = 2, LevelNumber = 2, LocationId = 1 },
                new Floor { Id = 3, LevelNumber = 1, LocationId = 2 },
                new Floor { Id = 4, LevelNumber = 2, LocationId = 2 },
                new Floor { Id = 5, LevelNumber = 3, LocationId = 2 },
                new Floor { Id = 6, LevelNumber = 1, LocationId = 3 }
            );

            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Name = "Room A", Capacity = 3, FloorId = 1 },
                new Room { Id = 2, Name = "Room A1", Capacity = 7, FloorId = 1 },
                new Room { Id = 3, Name = "Room B", Capacity = 2, FloorId = 2 },
                new Room { Id = 4, Name = "Room C", Capacity = 3, FloorId = 3 },
                new Room { Id = 5, Name = "Room C1", Capacity = 4, FloorId = 3 },
                new Room { Id = 6, Name = "Room C2", Capacity = 5, FloorId = 3 }
            );

            modelBuilder.Entity<Equipment>().HasData(
                new Equipment { Id = 1, Name = "USB-C Docking Station" },
                new Equipment { Id = 2, Name = "LED Desk Lamp" },
                new Equipment { Id = 3, Name = "Dual Monitor Arm" },
                new Equipment { Id = 4, Name = "HD Web Camera" },
                new Equipment { Id = 5, Name = "Mechanical Keyboard" }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Manager" },
                new Role { Id = 3, Name = "Admin" }
            );

            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "DepA", Description = "DepA for work" },
                new Department { Id = 2, Name = "DepB", Description = "DepB for work" },
                new Department { Id = 3, Name = "DebC", Description = "DebC for work" }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "Server=localhost;Database=OfficeDeskReservationDBSystem;Trusted_Connection=True;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}