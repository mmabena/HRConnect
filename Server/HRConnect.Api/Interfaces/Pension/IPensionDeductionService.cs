namespace HRConnect.Api.Interfaces.Pension
{
  using HRConnect.Api.DTOs.Payroll.Pension;

  public interface IPensionDeductionService
  {
    Task<PensionDeductionDto?> AddPensionDeductionAsync(PensionDeductionAddDto pensionDeductionAddDto);
    Task<List<PensionDeductionDto>> GetAllPensionDeductionsAsync();
    Task<PensionDeductionDto?> GetEmployeePensionDeductionByIdAsync(string employeeId);
    Task<List<PensionDeductionDto>> GetPensionDeductionsByPayRollRunIdAsync(int payrollRunId);
    Task<PensionDeductionDto?> UpdateEmployeePensionDeductionAsync(PensionDeductionUpdateDto pensionDeductionUpdateDto);
    Task PensionDeductionRollover();
  }
}
