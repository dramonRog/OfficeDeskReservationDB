using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Desk
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
        public ICollection<DeskEquipment> DeskEquipments { get; set; } = new List<DeskEquipment>();
    }
}
