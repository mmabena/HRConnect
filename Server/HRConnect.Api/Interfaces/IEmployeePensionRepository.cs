namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Threading.Tasks;

  public interface IEmployeePensionRepository
  {
    Task<Employee?> GetEmployeeByIdAsync(string employeeId);

    Task UpdateEmployeeAsync(Employee employee);
  }
}