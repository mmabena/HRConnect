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
        Task<Employee?> GetEmployeeByIdAsync(string EmployeeId);
        Task<Employee> CreateEmployeeAsync(Employee employeeModel);
        Task<Employee?> UpdateEmployeeAsync(Employee employeeModel);
        Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix);
        Task<bool> DeleteEmployeeAsync(string EmployeeId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<Employee?> GetEmployeeByEmailAsync(string email);
        Task<Employee?> GetEmployeeByEmailAsync(string email, string EmployeeId);
        Task<Employee?> GetEmployeeByIdNumberAsync(string idNumber);
        Task<Employee?> GetEmployeeByIdNumberAsync(string idNumber, string EmployeeId);
        Task<Employee?> GetEmployeeByPassportAsync(string passportNumber);
        Task<Employee?> GetEmployeeByPassportAsync(string passportNumber, string EmployeeId);
        Task<Employee?> GetEmployeeByTaxNumberAsync(string taxNumber);
        Task<Employee?> GetEmployeeByTaxNumberAsync(string taxNumber, string EmployeeId);
        Task<Employee?> GetEmployeeByContactNumberAsync(string contactNumber);
        Task<Employee?> GetEmployeeByContactNumberAsync(string contactNumber, string EmployeeId);
    }
}