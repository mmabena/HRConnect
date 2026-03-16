namespace HRConnect.Api.Interfaces
{
  using System.Threading.Tasks;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;
  public interface IPayrollPeriodService
  {
    Task<IEnumerable<PayrollPeriodDto>> GetAllPeriodsAsync();
    Task<PayrollPeriodDto?> GetPayrollPeriodByGuidAsync(int id);
    Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime);
    Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod);
    Task UpdateAsync(PayrollPeriod payrollPeriod);
    Task<PayrollPeriod?> GetLastPeriodAsync();
  }
}