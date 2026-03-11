namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.Payroll;
 using HRConnect.Api.Models.Payroll;


  public interface IPayrollRunRepository
  {
    Task<PayrollRunDto> GetByIdAsync(int id);
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    Task<PayrollRunDto> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<bool> HasFinalRunAsync(int id);
    Task<PayrollRun?> GetCurrentRunAsync();
    Task UpdateRunAsync(PayrollRun payrollRun);
  }
}