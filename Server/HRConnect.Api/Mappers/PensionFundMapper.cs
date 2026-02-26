namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  public static class PensionFundMapper
  {
    public static PensionFund ToPensionFundDto(this PensionFund entity)
    {
      return new PensionFund
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

    public static PensionFund ToEntity(PensionFundDto dto)
    {
      return new PensionFund
      {
        PensionFundId = dto.PensionFundId,
        EmployeeId = dto.EmployeeId,
        EmployeeName = dto.EmployeeName,
        MonthlySalary = dto.MonthlySalary,
        ContributionPercentage = dto.ContributionPercentage,
        ContributionAmount = dto.ContributionAmount,
        TaxCode = dto.TaxCode,
        PensionOptionId = dto.PensionOptionId,
      };
    }
  }
}
