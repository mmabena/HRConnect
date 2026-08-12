namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Threading.Tasks;

  public interface IEmployeePensionRepository
  {
    Task<Employee?> GetEmployeeByIdAsync(string employeeId, CancellationToken cancellationToken);
    Task<IEnumerable<Employee>> GetEmployeesAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken);




  }
}