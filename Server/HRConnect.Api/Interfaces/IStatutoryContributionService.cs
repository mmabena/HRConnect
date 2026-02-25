namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.StatutoryContribution;
  public interface IStatutoryContributionService
  {
    Task<IEnumerable<StatutoryContributionDto>> GetAllDeductionsAsync();
    Task<StatutoryContribution?> AddDeductionsAsync(string employeeId);
    Task<StatutoryContributionDto?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}

