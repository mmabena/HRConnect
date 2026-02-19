<<<<<<< HEAD
namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPayrollDeductionsService
  {
    Task<List<PayrollDeduction>> GetAllDeductionsAsync();
    Task<PayrollDeduction?> AddDeductionsAsync(string employeeId);
    Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
=======
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
>>>>>>> 6f925a0edeaed929a59e86c64f891a0419502b7b
}