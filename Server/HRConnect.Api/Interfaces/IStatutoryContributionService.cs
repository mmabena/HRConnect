namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.StatutoryContribution;
  using HRConnect.Api.Models;
  public interface IStatutoryContributionService
  {
    Task<IEnumerable<StatutoryContributionDto>> GetAllDeductionsAsync();
    Task<StatutoryContribution?> AddDeductionsAsync(string employeeId);
    Task<StatutoryContributionDto?> GetDeductionsByEmployeeIdAsync(string employeeId);
  }
}

