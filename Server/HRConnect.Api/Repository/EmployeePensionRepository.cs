namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class EmployeePensionRepository(ApplicationDBContext context) : IEmployeePensionRepository
  {
    public async Task<Employee?> GetEmployeeByIdAsync(string employeeId, CancellationToken cancellationToken)
    {
      return await context.Employees
          .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, cancellationToken);
    }

    public async Task UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken)
    {
      _ = context.Employees.Update(employee);
      _ = await context.SaveChangesAsync(cancellationToken);
    }
  }
}