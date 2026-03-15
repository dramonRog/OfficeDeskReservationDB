using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Floor
    {
        public int Id { get; set; }
        public int LevelNumber { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
