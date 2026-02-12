namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class EmployeeRepository : IEmployeeRepository
  {
    private readonly ApplicationDBContext _context;
    public EmployeeRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
      return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string emloyeeCode)
    {
      return await _context.Employees.
      FirstOrDefaultAsync(e => e.EmployeeCode == emloyeeCode);
    }
    public async Task<List<Employee>> GetEmployeesAsync()
    {
      return await _context.Employees.ToListAsync();
    }

    public async Task<Employee?> UpdateEmployeeAsync(int employeeId, Employee employeeModel)
    {
      var existingEmployee = await _context.Employees.FindAsync(employeeId);

      if (existingEmployee == null) return null;

      _context.Entry(existingEmployee).CurrentValues.SetValues(employeeModel);
      _ = await _context.SaveChangesAsync();
      return existingEmployee;
    }
  }
}
