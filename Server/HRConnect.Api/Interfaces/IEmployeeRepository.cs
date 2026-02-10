namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee> CreateEmployeeAsync(Employee employeeModel);
        Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix);
        
    }
}