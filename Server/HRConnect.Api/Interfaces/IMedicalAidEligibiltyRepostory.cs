namespace HRConnect.Api.Interfaces
{
  using DTOs;

  public interface IMedicalAidEligibilityRepository
  {
    Task<IReadOnlyList<ResponseEligibileOptionsDto>> GetEmployeeEligibleMedicalOptionsAsync(string employeeId, RequestEligibileOptionsDto payload);
    //Task
  }
}
