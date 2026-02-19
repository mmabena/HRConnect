namespace HRConnect.Api.Models.MedicalOptions.Records
{
  public record SalaryBracketValidatorRecord(
    int optionId,
    string optionName,
    decimal? salaryBracketMin, // null = starts from 0
    decimal? salaryBracketMax // null = uncapped
  );
}