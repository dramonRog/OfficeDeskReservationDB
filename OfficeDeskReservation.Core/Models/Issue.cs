using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDeskReservationDB.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime ReportedAt { get; set; }
        public bool IsResolved { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int DeskId { get; set; }
        public Desk Desk { get; set; }
    }
}
