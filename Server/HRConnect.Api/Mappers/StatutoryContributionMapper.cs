namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.StatutoryContribution;
  using HRConnect.Api.Models;

  public static class StatutoryContributionMapper
  {
    public static StatutoryContributionDto ToPayrollDeductionDto(this StatutoryContribution statutoryContributionModel)
    {
      return new StatutoryContributionDto
      {
        EmployerSdlContribution = statutoryContributionModel.EmployerSdlContribution,
        UifEmployeeAmount = statutoryContributionModel.UifEmployeeAmount,
        UifEmployerAmount = statutoryContributionModel.UifEmployerAmount,
        EmployeeId = statutoryContributionModel.EmployeeId,
        IdNumber = statutoryContributionModel.IdNumber,
        PassportNumber = statutoryContributionModel.PassportNumber,
        MonthlySalary = statutoryContributionModel.MonthlySalary,
        CurrentMonth = statutoryContributionModel.CurrentMonth
      };
    }
    public static StatutoryContribution ToPayrollDeductionsFromDto(this StatutoryContributionDto statutoryContributionDto)
    {
      return new StatutoryContribution
      {
        UifEmployeeAmount = statutoryContributionDto.UifEmployeeAmount,
        EmployerSdlContribution = statutoryContributionDto.EmployerSdlContribution,
        UifEmployerAmount = statutoryContributionDto.UifEmployerAmount,
        EmployeeId = statutoryContributionDto.EmployeeId,
        IdNumber = statutoryContributionDto.IdNumber,
        PassportNumber = statutoryContributionDto.PassportNumber,
        CurrentMonth = statutoryContributionDto.CurrentMonth,
        MonthlySalary = statutoryContributionDto.MonthlySalary
      };
    }
  }
}