namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Employee;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage;

  public interface IEmployeeRepository
  {
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(string employeeId);
    Task<Employee> CreateEmployeeAsync(Employee employeeModel);
    Task<Employee?> UpdateEmployeeAsync(Employee employeeModel);
    Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix);
    Task<bool> DeleteEmployeeAsync(string employeeId);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<Employee?> GetEmployeeByEmailAsync(string email);
    Task<Employee?> GetEmployeeByEmailAsync(string email, string employeeId);
    Task<Employee?> GetEmployeeByIdNumberAsync(string idNumber);
    Task<Employee?> GetEmployeeByIdNumberAsync(string idNumber, string employeeId);
    Task<Employee?> GetEmployeeByPassportAsync(string passportNumber);
    Task<Employee?> GetEmployeeByPassportAsync(string passportNumber, string employeeId);
    Task<Employee?> GetEmployeeByTaxNumberAsync(string taxNumber);
    Task<Employee?> GetEmployeeByTaxNumberAsync(string taxNumber, string employeeId);
    Task<Employee?> GetEmployeeByContactNumberAsync(string contactNumber);
    Task<Employee?> GetEmployeeByContactNumberAsync(string contactNumber, string EmployeeId);
  }
}