using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
