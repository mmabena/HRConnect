namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.StatutoryContribution;
  using HRConnect.Api.Models;

  public static class StatutoryContributionMapper
  {
    public static StatutoryContributionDto StatutoryContributionDto(this StatutoryContribution payrollDeductionModel)
    {
      return new StatutoryContributionDto
      {
        EmployerSdlContribution = payrollDeductionModel.EmployerSdlContribution,
        UifEmployeeAmount = payrollDeductionModel.UifEmployeeAmount,
        UifEmployerAmount = payrollDeductionModel.UifEmployerAmount,
        EmployeeId = payrollDeductionModel.EmployeeId,
        IdNumber = payrollDeductionModel.IdNumber,
        PassportNumber = payrollDeductionModel.PassportNumber,
        MonthlySalary = payrollDeductionModel.MonthlySalary
      };
    }
    public static StatutoryContribution ToStatutoryContributionFromDto(this StatutoryContributionDto payrollDeductionsDto)
    {
      return new StatutoryContribution
      {
        UifEmployeeAmount = payrollDeductionsDto.UifEmployeeAmount,
        EmployerSdlContribution = payrollDeductionsDto.EmployerSdlContribution,
        UifEmployerAmount = payrollDeductionsDto.UifEmployerAmount,
        EmployeeId = payrollDeductionsDto.EmployeeId,
        IdNumber = payrollDeductionsDto.IdNumber,
        PassportNumber = payrollDeductionsDto.PassportNumber,
        MonthlySalary = payrollDeductionsDto.MonthlySalary
      };
    }
  }
}