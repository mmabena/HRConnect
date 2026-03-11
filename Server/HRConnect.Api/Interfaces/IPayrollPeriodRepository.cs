namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.Payroll;
 using HRConnect.Api.Models.Payroll;



  public interface IPayrollPeriodRepository
  {
    Task<PayrollPeriodDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriod();
    Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime);
    Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod);
  }
}