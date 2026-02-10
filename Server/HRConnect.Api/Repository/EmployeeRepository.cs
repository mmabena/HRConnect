namespace HRConnect.Api.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using Microsoft.EntityFrameworkCore;

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDBContext _context;
        public EmployeeRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employeeModel)
        {
            await _context.Employees.AddAsync(employeeModel);
            await _context.SaveChangesAsync();
            return employeeModel;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix)
        {
            return await _context.Employees
                    .Where(e => e.EmployeeId.StartsWith(prefix))
                    .Select(e => e.EmployeeId)
                    .ToListAsync();
        }
        
    }
}