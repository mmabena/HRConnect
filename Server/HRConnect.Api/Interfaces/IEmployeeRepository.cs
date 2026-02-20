
namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  public interface IEmployeeRepository
  {
    Task<Employee?> GetEmployeeByIdAsync(string employeeId);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> UpdateEmployeeAsync(string employeeId, Employee employeeModel);
  }
}