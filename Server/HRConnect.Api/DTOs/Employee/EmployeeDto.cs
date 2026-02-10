namespace HRConnect.Api.DTOs.Employee
{
    public class EmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhysicalAddress { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public DateOnly StartDate { get; set; }
        public string Branch { get; set; } = string.Empty;
        public decimal MonthlySalary { get; set; }
        public int  PositionId { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string CareerManager { get; set; } = string.Empty;
        public string EmpPicture { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}