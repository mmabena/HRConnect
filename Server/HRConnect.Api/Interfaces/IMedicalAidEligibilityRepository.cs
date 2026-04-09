namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;

  public interface IMedicalAidEligibilityRepository
  {
    Task<IReadOnlyList<ResponseEligibileOptionsDto>> GetEmployeeEligibleMedicalOptionsAsync(
      string employeeId, RequestEligibileOptionsDto payload);
  }
}
