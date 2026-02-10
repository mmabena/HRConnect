namespace HRConnect.Api.Interfaces.Finance
{
  using HRConnect.Api.DTOs.Employee.Pension;

  public interface IPensionProjectionService
  {
    PensionProjectionResultDto ProjectPension(PensionProjectionRequestDto pensionProjectRequestDto);
  }
}
