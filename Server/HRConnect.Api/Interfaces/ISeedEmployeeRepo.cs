
namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface ISeedEmployeeRepo
  {
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<List<Employee>> GetEmployeesAsync();
    Task<Employee?> GetEmployeeByCodeAsync(string emloyeeCode);
    Task<Employee?> UpdateEmployeeAsync(int employeeId, Employee employeeModel);
  }
}