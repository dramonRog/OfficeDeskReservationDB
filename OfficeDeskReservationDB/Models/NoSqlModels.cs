namespace OfficeDeskReservationDB.Models
{
    public class NoSqlUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class NoSqlDesk
    {
        public string DeskName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int FloorLevel { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public List<string> AssignedEquipments { get; set; } = new();
    }

    public class NoSqlReservation
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string DeskName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
    }

    public class NoSqlIssue
    {
        public string Description { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public bool IsResolved { get; set; }
        public string ReportedBy { get; set; } = string.Empty;
        public string DeskName { get; set; } = string.Empty;
    }
}
