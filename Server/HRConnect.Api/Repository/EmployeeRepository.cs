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

    public async Task<Employee?> GetEmployeeByIdAsync(string EmployeeId)
    {
      return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId);
    }
    public async Task<Employee?> UpdateEmployeeAsync(string employeeId, Employee employeeModel)
    {
      var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

      if (existingEmployee == null) return null;

      _context.Entry(existingEmployee).CurrentValues.SetValues(employeeModel);
      _ = await _context.SaveChangesAsync();

      return existingEmployee;
    }
  }
}