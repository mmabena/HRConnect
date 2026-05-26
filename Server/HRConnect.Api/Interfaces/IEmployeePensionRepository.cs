namespace HRConnect.Api.Interfaces
{
  using System.Threading;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;

  public interface IEmployeePensionRepository
  {
    Task<Employee?> GetEmployeeByIdAsync(string employeeId, CancellationToken cancellationToken);

    Task UpdateEmployeeAsync(Employee employee, CancellationToken cancellationToken);
  }
}