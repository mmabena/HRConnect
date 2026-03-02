namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public interface IPensionRepository
  {
    // ============================
    // Pension Funds
    // ============================
    Task<IEnumerable<PensionFund>> GetPensionFundsAsync();
    Task<PensionFund?> GetPensionFundByIdAsync(int id);
    Task AddPensionFundAsync(PensionFund fund);
    Task UpdatePensionFundAsync(PensionFund fund);

    // NEW: Add or update PensionFund for an employee
    Task AddOrUpdatePensionFundAsync(PensionFund fund);

    // NEW: Explicit save method
    Task SaveChangesAsync();

    // ============================
    // Pension Options
    // ============================
    Task<IEnumerable<PensionOption>> GetPensionOptionsAsync();
    Task<PensionOption?> GetPensionOptionByIdAsync(int id);
    Task AddPensionOptionAsync(PensionOption pensionoption);
    Task UpdatePensionOptionAsync(PensionOption pensionoption);

    // ============================
    // Employees
    // ============================
    Task<Employee?> GetEmployeeByIdAsync(string id); // FIX: string not int
  }
}

