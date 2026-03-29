using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }

        public int FloorId { get; set; }
        public Floor Floor { get; set; }

        public ICollection<Desk> Desks { get; set; } = new List<Desk>();
    }
}
