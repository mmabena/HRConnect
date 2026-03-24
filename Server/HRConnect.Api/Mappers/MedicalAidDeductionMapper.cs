namespace HRConnect.Api.Mappers
{
  using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
  using Models.PayrollDeduction;

  public static class MedicalAidDeductionMapper
  {
    public static MedicalAidDeduction ToMedicalAidDeductionDto(this MedicalAidDeductionDto dto)
    {
      return new MedicalAidDeduction
      {
        Id = dto.MedicalAidDeductionId,
        PayrollRunId = dto.PayrollRunId,
        EmployeeId = dto.EmployeeId,
        Name = dto.Name,
        Surname = dto.Surname,
        Branch = dto.Branch,
        Salary = dto.Salary,
        EmployeeStartDate = dto.EmployeeStartDate,
        TerminationDate = dto.TerminationDate,
        EffectiveDate = dto.EffectiveDate,
        MedicalOptionId = dto.MedicalOptionId, // TODO: Use Name in place - There an endpoint to get option by Name is required
        MedicalCategoryId = dto.MedicalCategoryId, // Use Name in place - There an endpoint to get option category by Name is required
        PrincipalCount = dto.PrincipalCount,
        AdultCount = dto.AdultCount,
        ChildrenCount = dto.ChildrenCount,
        PrincipalPremium = dto.PrincipalPremium,
        SpousePremium = dto.SpousePremium,
        ChildPremium = dto.ChildPremium,
        TotalDeductionAmount = dto.TotalDeductionAmount,
        CreatedDate = dto.CreatedDate,
        IsActive = dto.IsActive,
        TerminationReason = dto.TerminationReason
      };
    }
  }
}