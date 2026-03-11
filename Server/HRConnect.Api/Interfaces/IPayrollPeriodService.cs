namespace HRConnect.Api.Interfaces
{
  using DTOs.Payroll;
  using Models.Payroll;

  public interface IPayrollPeriodService
  {
    Task<IEnumerable<PayrollPeriodDto>> GetAllPeriodsAsync();
    Task<PayrollPeriodDto?> GetPayrollPeriodByGuidAsync(int id);
    Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime);
    Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod);
    Task UpdateAsync(PayrollPeriod payrollPeriod);
    Task<PayrollPeriod?> GetLastPeriodAsync();
    Task<PayrollPeriod?> GetCurrentActivePayrollPeriod();
  }
}