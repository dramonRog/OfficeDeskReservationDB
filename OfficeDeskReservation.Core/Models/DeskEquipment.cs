using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class DeskEquipment
    {
        public int DeskId { get; set; }
        public Desk Desk { get; set; }

        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }

        public int Quantity { get; set; }
    }
}
