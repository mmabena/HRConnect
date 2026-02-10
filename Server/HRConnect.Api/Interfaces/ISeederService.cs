namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;

  public interface ISeederService
  {
    Task SeedEmployeeAsync();
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<List<Employee>> GetEmployeesAsync();
  }
}