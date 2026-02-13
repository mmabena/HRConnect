namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPayrollDeductionsService
  {
    Task<List<PayrollDeduction>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> AddDeductionsAsync(string employeeId);
    Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}