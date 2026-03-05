namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Models;

  public interface IPayrollPeriodRepository
  {
    Task<PayrollPeriodDto?> GetByIdAsync(Guid id);
    Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime);
    Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod);
  }
}