namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.PayrollDeduction;
  public interface IPayrollDeductionsService
  {
    Task<IEnumerable<PayrollDeductionDto>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> AddDeductionsAsync(string employeeId);
    Task<PayrollDeductionDto?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}