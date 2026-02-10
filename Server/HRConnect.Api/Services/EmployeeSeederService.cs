namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.ChangeTracking;

  public class EmployeeSeederService : ISeederService
  {
    private readonly ApplicationDBContext _context;
    private readonly ISeedEmployeeRepo _seedEmployeeRepo;
    private readonly List<Employee> _seedEmployees = new()
    {
      new Employee
      {
          // Id = 1,
          Name = "Worker",
          EmployeeCode = "WOR001",
          MonthlySalary = 8500m
      },
      new Employee
      {
          // Id = 2,
          Name = "Sideman",
          EmployeeCode = "SID002",
          MonthlySalary = 12500m
      },
      new Employee
      {
          // Id =3,
          Name = "Bossman",
          EmployeeCode = "BOS003",
          MonthlySalary = 35000m
      }
      ,
      new Employee
      {
          Name = "CEO",
          EmployeeCode = "CEO004",
          MonthlySalary=1771200m
      }
    };
    public EmployeeSeederService(ApplicationDBContext context, ISeedEmployeeRepo seedEmployeeRepo)
    {
      _context = context;
      _seedEmployeeRepo = seedEmployeeRepo;
    }

    public async Task SeedEmployeeAsync()
    {
      List<string> existingCodes = await _context.Employees.Select(e => e.EmployeeCode)
            .ToListAsync();
      List<Employee> employeesToCreate = _seedEmployees.Where(e => !existingCodes.Contains(e.EmployeeCode)).ToList();

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

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
      return await _seedEmployeeRepo.GetEmployeeByIdAsync(id);
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
      return await _seedEmployeeRepo.GetEmployeesAsync();
    }
  }
}