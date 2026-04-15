namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  public static class PensionFundMapper
  {
    public static PensionFundDto ToDto(this PensionFund entity)
    {
      return new PensionFundDto
      {
        PensionFundId = entity.PensionFundId,
        EmployeeId = entity.EmployeeId,
        EmployeeName = entity.EmployeeName,
        MonthlySalary = entity.MonthlySalary,
        ContributionPercentage = entity.ContributionPercentage,
        ContributionAmount = entity.ContributionAmount,
        TaxCode = entity.TaxCode,
        PensionOptionId = entity.PensionOptionId
      };
    }

    public static PensionFund ToEntity(this PensionFundDto dto)
    {
      return new PensionFund
      {
        PensionFundId = dto.PensionFundId,
        EmployeeId = dto.EmployeeId,
        EmployeeName = dto.EmployeeName,
        MonthlySalary = dto.MonthlySalary,
        ContributionPercentage = dto.ContributionPercentage,
        ContributionAmount = dto.ContributionAmount,
        TaxCode = (int)dto.TaxCode, 
        PensionOptionId = dto.PensionOptionId
      };
    }
  }
}
