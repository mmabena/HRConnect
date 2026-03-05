namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Models;

  public interface IPayrollRunRepository
  {
    Task<PayrollRunDto> GetByIdAsync(int id);
    /// <summary>
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    /// </summary>
    /// <param name="payrollRun"></param>
    /// <returns></returns>
    Task<PayrollRunDto> CreatePayrollRunAsync(PayrollRun payrollRun);
    Task<bool> HasFinalRunAsync(int id);
    Task<PayrollRun?> GetCurrentRunAsync();
  }
}