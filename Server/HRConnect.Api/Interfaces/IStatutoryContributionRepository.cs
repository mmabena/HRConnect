namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IStatutoryContributionRepository
  {
    Task<StatutoryContribution> AddDeductionsAsync(StatutoryContribution statutoryContribution);
    Task<IEnumerable<StatutoryContribution>> GetAllDeductionsAsync();
    Task<StatutoryContribution?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}