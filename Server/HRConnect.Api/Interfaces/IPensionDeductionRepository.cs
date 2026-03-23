namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.PayrollDeduction;

  public interface IPensionDeductionRepository
  {
    Task<PensionDeduction> AddAsync(PensionDeduction pensionDeduction);
    Task<List<PensionDeduction>> GetAllAsync();
    Task<PensionDeduction?> GetByEmployeeIdAsync(string employeeId);
    Task<PensionDeduction?> GetByEmployeeIdAndIsNotLockedAsync(string employeeId);
    Task<List<PensionDeduction>> GetByPayRollRunIdAsync(int payrollRunId);
    Task<PensionDeduction?> GetByEmployeeIdAndLastRunIdAsync(string employeeId, int payRollRunId);
    Task<PensionDeduction> UpdateAsync(PensionDeduction pensionDeduction);
    Task<bool> DeleteAsync();
  }
}
