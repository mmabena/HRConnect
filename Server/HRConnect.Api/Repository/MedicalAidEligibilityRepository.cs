namespace HRConnect.Api.Repository
{
  using DTOs;
  using Interfaces;

  public class MedicalAidEligibilityRepository : IMedicalAidEligibilityRepository
  {
    private readonly IEmployeeRepository _employeeRepository;
    //private readonly 
    public MedicalAidEligibilityRepository(IEmployeeRepository employeeRepository)
    {
      _employeeRepository = employeeRepository;
    }

    public async Task<IReadOnlyList<ResponseEligibileOptionsDto>> GetEmployeeEligibleMedicalOptionsAsync(string employeeId, RequestEligibileOptionsDto payload)
    {
      throw new NotImplementedException();
    }
  }
}