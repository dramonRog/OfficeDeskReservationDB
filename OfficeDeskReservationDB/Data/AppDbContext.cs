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
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "Server=localhost;Database=OfficeDeskReservationDB;Trusted_Connection=True;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}