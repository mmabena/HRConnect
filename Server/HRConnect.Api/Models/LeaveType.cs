namespace HRConnect.Api.Models
{
    /// <summary>
    /// Defines the types of leave supported by the system
    /// (Annual, Sick, Maternity, Family Responsibility).
    /// </summary>
    public class LeaveType
    {
        public int LeaveTypeId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int DaysEntitled { get; set; }
    }
}