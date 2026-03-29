using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }

        public ICollection<Floor> Floors { get; set; } = new List<Floor>();
    }
}
