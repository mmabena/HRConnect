namespace HRConnect.Api.Interfaces.Payroll.Earning
{
  using HRConnect.Api.DTOs.Payroll.Earning;

  public interface IPayrollEarningService
  {
    Task<PayrollEarningDto> AddPayrollEarningAsync(PayrollEarningAddDto payrollEarningAddDto);
    Task<List<PayrollEarningDto>> GetAllPayrollEarningsAsync();
    Task<PayrollEarningDto?> GetPayrollEarningByIdAsync(string payrollEarningId);
    Task<List<PayrollEarningDto>> GetPayrollEarningByTaxCode(int taxCode);
    Task<PayrollEarningDto> UpdatePayrollEarningAsync(PayrollEarningUpdateDto payrollEarningUpdateDto);
    Task<string> SetPayrollEarningToInactiveAsync(string payrollEarningId);
  }
}
