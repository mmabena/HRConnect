namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.Employee;

    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllEmployeesAsync();

        Task<EmployeeDto?> GetEmployeeByIdAsync(string employeeId);

        Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto);

        Task<EmployeeDto?> UpdateEmployeeAsync(string employeeId, UpdateEmployeeRequestDto employeeDto);

        Task<bool> DeleteEmployeeAsync(string employeeId);

        Task<EmployeeDto?> GetEmployeeByEmailAsync(string employeeEmail);
    }
}