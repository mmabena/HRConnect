namespace HRConnect.Api.DTOs.PayrollDeductions
{
    public class CreateStatutoryContributionDto
    {
        public int EmployeeId { get; set; }
        public decimal MonthlySalary { get; set; }
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public decimal SdlAmount { get; set; }
        public decimal UifEmployeeAmount { get; set; }
        public decimal UifEmployerAmount { get; set; }
    }
}