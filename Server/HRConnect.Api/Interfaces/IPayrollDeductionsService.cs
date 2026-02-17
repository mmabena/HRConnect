namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.PayrollDeductions;
  public interface IPayrollDeductionsService
  {
    Task<List<PayrollDeductionsDto>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> AddDeductionsAsync(string employeeId);
    Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}