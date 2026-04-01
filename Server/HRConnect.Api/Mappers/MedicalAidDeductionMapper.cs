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
        MedicalOptionId = dto.MedicalOptionId, 
        MedicalCategoryId = dto.MedicalCategoryId, 
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
    
    public static UpdateMedicalAidDeductionResponseDto ToUpdateMedicalAidDeductionResponseDto(MedicalAidDeduction response)
    {
      return new UpdateMedicalAidDeductionResponseDto
      {
        Id = response.Id,
        PayrollRunId = response.Id,
        EmployeeId = response.EmployeeId ?? string.Empty,
        Name = response.Name,
        Surname = response.Surname,
        Branch = response.Branch,
        Salary = response.Salary,
        EmployeeStartDate = response.EmployeeStartDate,
        EffectiveDate = response.EffectiveDate,
        MedicalOptionId = response.MedicalOptionId,
        OptionName = response.OptionName,
        MedicalCategoryId = response.MedicalCategoryId,
        OptionCategoryName = response.OptionCategoryName,
        PrincipalCount = response.PrincipalCount,
        AdultCount = response.AdultCount,
        ChildrenCount = response.ChildrenCount,
        PrincipalPremium = response.PrincipalPremium,
        SpousePremium = response.SpousePremium,
        ChildPremium = response.ChildPremium,
        TotalDeductionAmount = response.TotalDeductionAmount,
        CreatedDate = response.CreatedDate,
        IsActive = response.IsActive,
        UpdatedDate = response.UpdatedDate,
        TerminationDate = response.TerminationDate,
        TerminationReason = response.TerminationReason,
      };
    }
    
    /// <summary>
    /// Maps a MedicalAidDeduction entity to a MedicalAidDeductionDto.
    /// </summary>
    public static MedicalAidDeductionDto MapToDto(MedicalAidDeduction request)
    {
      return new MedicalAidDeductionDto
      {
        PayrollRunId = request.Id,
        EmployeeId = request.EmployeeId ?? string.Empty,
        Name = request.Name,
        Surname = request.Surname,
        Branch = request.Branch,
        Salary = request.Salary,
        EmployeeStartDate = request.EmployeeStartDate,
        EffectiveDate = request.EffectiveDate,
        MedicalOptionId = request.MedicalOptionId,
        MedicalCategoryId = request.MedicalCategoryId,
        PrincipalCount = request.PrincipalCount,
        AdultCount = request.AdultCount,
        ChildrenCount = request.ChildrenCount,
        PrincipalPremium = request.PrincipalPremium,
        SpousePremium = request.SpousePremium,
        ChildPremium = request.ChildPremium,
        TotalDeductionAmount = request.TotalDeductionAmount,
        CreatedDate = request.CreatedDate,
        IsActive = request.IsActive,
        UpdatedDate = request.UpdatedDate
      };
    }
  }
}