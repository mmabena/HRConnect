namespace HRConnect.Api.DTOs
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.DTOs.Payroll.Pension;
  public class PensionOptionDto
  {
    public int PensionOptionId { get; set; }
    public decimal ContributionPercentage { get; set; }
    public string? PensionFundName { get; set; }

    public ICollection<EmployeeDto> Employees { get; set; }

    public ICollection<EmployeePensionEnrollmentDto> EmployeePensionEnrollment { get; set; }

    public ICollection<PensionDeductionDto> PensionDeduction { get; set; }
  }
}
