namespace HRConnect.Api.DTOs.SeedEmployee
{
  public class SeedEmployeeDto
  {
    public string Name { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal MonthlySalary { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
  }
}