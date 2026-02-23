namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.ChangeTracking;

  public class EmployeeSeederService
  {
    private readonly ApplicationDBContext _context;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly List<Employee> _seedEmployees = new()
    {
      new Employee
      {
          // Id = 1,
          Name = "Worker",
          EmployeeId = "WOR001",
          IdNumber="0201025054080",
          MonthlySalary = 8500m
      },
      new Employee
      {
          // Id = 1,
          Name = "NPC",
          EmployeeId = "NPC001",
          IdNumber="0201025054080",
          MonthlySalary =0m
      },
      new Employee
      {
          // Id = 2,
          Name = "Sideman",
          IdNumber="0210205004080",
          EmployeeId = "SID002",
          MonthlySalary = 12500m
      },
      new Employee
      {
          // Id =3,
          Name = "Bossman",
          EmployeeId = "BOS003",
          MonthlySalary = 35000m,
          IdNumber="0201025054080"
      }
      ,
      new Employee
      {
          Name = "CEO",
          PassportNumber="12345",
          EmployeeId = "CEO004",
          MonthlySalary=1771200m
      }
    };
    public EmployeeSeederService(ApplicationDBContext context, IEmployeeRepository employeeRepo)
    {
      _context = context;
      _employeeRepo = employeeRepo;
    }

    public async Task SeedEmployeeAsync()
    {
      var existingCodes = await _context.Employees.Select(e => e.EmployeeId)
            .ToListAsync();
      var employeesToCreate = _seedEmployees.Where(e => !existingCodes.Contains(e.EmployeeId)).ToList();

      Console.WriteLine($"FOR EACH Count {employeesToCreate.Count}");
      if (employeesToCreate.Count == 0)
      {
        return;
      }

      EntityEntry<Employee>? result = null;
      foreach (Employee employee in employeesToCreate)
      {
        Console.WriteLine($"Employer Being Seeded {employee.Name}");
        result = await _context.Employees.AddAsync(employee);
        Console.WriteLine($"SEED RESULTS {result.State}");
      }

      //this generates IDs
      _ = await _context.SaveChangesAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(string id)
    {
      return await _employeeRepo.GetEmployeeByIdAsync(id);
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
      return await _employeeRepo.GetAllEmployeesAsync();
    }
  }
}