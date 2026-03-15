using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<DeskEquipment> DeskEquipments { get; set; } = new List<DeskEquipment>();
    }
}
