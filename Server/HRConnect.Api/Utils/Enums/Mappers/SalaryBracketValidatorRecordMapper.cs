namespace HRConnect.Api.Utils.Enums.Mappers
{
  
  using DTOs.MedicalOption;
  using Models.MedicalOptions.Records;

  public static class SalaryBracketValidatorRecordMapper
  {
    public static SalaryBracketValidatorRecord ToSalaryBracketValidatorRecord(
      this UpdateMedicalOptionVariantsDto entity, string entityOptionName)
    {
      return new SalaryBracketValidatorRecord
      (
        entity.MedicalOptionId,
        entityOptionName,
        (decimal)entity.SalaryBracketMin,
        (decimal)entity.SalaryBracketMax
      );
    }
  }  
}
