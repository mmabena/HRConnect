namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Employee;

  public interface IEmployeeService
  {
    Task<List<EmployeeDto>> GetAllEmployeesAsync(int userId);
    Task<EmployeeDto?> GetEmployeeByIdInternalAsync(string employeeId);
    Task<EmployeeDto?> GetEmployeeByIdAsync(int userId, string employeeId);
    Task<List<EmployeeDto>> GetAllEmployeesByCompanyAsync(string companyId);
    Task<EmployeeDto> CreateEmployeeAsync(int userId, CreateEmployeeRequestDto employeeRequestDto);

    Task<EmployeeDto?> UpdateEmployeeAsync(int userId, string employeeId, UpdateEmployeeRequestDto employeeDto);

    Task<bool> DeleteEmployeeAsync(int userId, string employeeId);

    Task ValidateEmployeeAsync(int userId, CreateEmployeeRequestDto employeeDto);

    Task<EmployeeDto?> GetEmployeeByEmailAsync(string employeeEmail);
  }
}