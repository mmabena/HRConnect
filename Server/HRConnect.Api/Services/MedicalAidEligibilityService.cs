namespace HRConnect.Api.Services
{
  using DTOs.MedicalOption;
  using Interfaces;

  public class MedicalAidEligibilityService : IMedicalAidEligibilityService
  {
    private readonly IEmployeeService _employeeService;
    public MedicalAidEligibilityService(IEmployeeService employeeService)
    {
      _employeeService = employeeService;
    }
    public Task<IReadOnlyList<MedicalOptionCategoryDto>> GetEligibleMedicalOptionsForEmployee(string employeeId)
    {
      var employeeData = _employeeService.GetEmployeeByIdAsync(employeeId);
      
      
      throw new NotImplementedException();
    }
  }
}
