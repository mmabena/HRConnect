namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.Employee;
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(string EmployeeId);
    Task<EmployeeDto?> GetEmployeeByEmailAsync(string employeeEmail);
        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto);
        Task<EmployeeDto?> UpdateEmployeeAsync(string EmployeeId, UpdateEmployeeRequestDto employeeDto);
        Task<bool> DeleteEmployeeAsync(string EmployeeId);

  }
}