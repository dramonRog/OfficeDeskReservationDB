using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }
}
