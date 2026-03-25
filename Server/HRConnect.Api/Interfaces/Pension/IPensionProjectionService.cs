namespace HRConnect.Api.Interfaces.Pension
{
  using HRConnect.Api.DTOs.Employee.Pension;

  public interface IPensionProjectionService
  {
    PensionProjectionResultDto ProjectPension(PensionProjectionRequestDto pensionProjectRequestDto);
  }
}
