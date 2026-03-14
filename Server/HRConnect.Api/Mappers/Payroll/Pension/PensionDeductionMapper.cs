namespace HRConnect.Api.Mappers.Payroll.Pension
{
  using HRConnect.Api.DTOs.Payroll.Pension;
  using HRConnect.Api.Models.PayrollDeduction;

  public static class PensionDeductionMapper
  {
    public static PensionDeductionDto ToPensionDeductionDTO(this PensionDeduction pensionDeduction)
    {
      return new PensionDeductionDto
      {
        FirstName = pensionDeduction.FirstName,
        LastName = pensionDeduction.LastName,
        DateJoinedCompany = pensionDeduction.DateJoinedCompany,
        IDNumber = pensionDeduction.IDNumber,
        Passport = pensionDeduction.Passport,
        TaxNumber = pensionDeduction.TaxNumber,
        PensionableSalary = pensionDeduction.PensionableSalary,
        PensionOptionId = pensionDeduction.PensionOptionId,
        PendsionCategoryPercentage = pensionDeduction.PendsionCategoryPercentage,
        PensionContribution = pensionDeduction.PensionContribution,
        VoluntaryContribution = pensionDeduction.VoluntaryContribution,
        EmailAddress = pensionDeduction.EmailAddress,
        PhyscialAddress = pensionDeduction.PhyscialAddress,
        PayrollRunId = pensionDeduction.PayrollRunId,
        CreatedDate = pensionDeduction.CreatedDate,
        IsActive = pensionDeduction.IsActive,
      };
    }
  }
}
