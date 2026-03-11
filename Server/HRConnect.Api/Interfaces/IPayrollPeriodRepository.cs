namespace HRConnect.Api.Interfaces
{
  using DTOs.Payroll;
  using Models.Payroll;

  public interface IPayrollPeriodRepository
  {
    Task<PayrollPeriodDto?> GetByIdAsync(int id);
    Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriod();
    Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime);
    Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod);
    Task UpdateAsync(PayrollPeriod payrollPeriod);
    Task<PayrollPeriod?> GetLastPeriodAsync();
    Task<PayrollPeriod?> GetCurrentActivePayrollPeriod();
  }
}