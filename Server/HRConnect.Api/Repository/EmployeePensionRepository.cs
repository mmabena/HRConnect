namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class EmployeePensionRepository(ApplicationDBContext context) : IEmployeePensionRepository
  {
    public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
    {
      return await context.Employees
          .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
      _ = context.Employees.Update(employee);
      _ = await context.SaveChangesAsync();
    }
  }
}