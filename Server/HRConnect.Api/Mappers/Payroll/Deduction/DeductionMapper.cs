namespace HRConnect.Api.Mappers.Payroll.Deduction
{
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Models.PayrollDeduction;


  public static class DeductionMapper
  {
    public static DeductionDto ToDeductionDto(this Deduction deduction)
    {
      return new DeductionDto
      {
        DeductionId = deduction.DeductionId,
        CompanyId = deduction.CompanyId,
        TaxCode = deduction.TaxCode,
        ShortDescription = deduction.ShortDescription,
        LongDescription = deduction.LongDescription,
        DeductionType = deduction.DeductionType,
        InputType = deduction.InputType,
        MinimumValue = deduction.MinimumValue,
        MaximumValue = deduction.MaximumValue,
        EmployerContributed = deduction.EmployerContributed,
        Status = deduction.Status,
        ModifiedDate = deduction.ModifiedDate
      };
    }

    public static Deduction ToDeductionModel(this DeductionAddDto deductionAddDto)
    {
      return new Deduction
      {
        DeductionId = "",
        CompanyId = deductionAddDto.CompanyId,
        TaxCode = deductionAddDto.TaxCode,
        ShortDescription = deductionAddDto.ShortDescription,
        LongDescription = deductionAddDto.LongDescription,
        DeductionType = deductionAddDto.DeductionType,
        InputType = deductionAddDto.InputType,
        MinimumValue = deductionAddDto.MinimumValue,
        MaximumValue = deductionAddDto.MaximumValue,
        EmployerContributed = deductionAddDto.EmployerContributed
      };
    }
  }
}
