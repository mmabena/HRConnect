namespace HRConnect.Api.Interfaces.PensionProjection
{
  using HRConnect.Api.DTOs.Employee.Pension;

  public interface IPensionProjectionService
  {
    PensionProjectionResultDto ProjectPension(PensionProjectionRequestDto pensionProjectRequestDto);
  }
}
