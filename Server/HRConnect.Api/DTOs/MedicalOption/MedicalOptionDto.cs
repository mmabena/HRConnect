namespace HRConnect.Api.DTOs.MedicalOption
{
  /// <summary>
  /// The Medical Option Data Transfer Object will be used in instances where
  ///  we need to transfer a single instance of a medical option to the client
  /// Thus will contain all details of a specific medical option will be sent to the client
  /// </summary>
  
  public class MedicalOptionDto
  {
    public int MedicalOptionId { get; set; }
    public string MedicalOptionName { get; set; } = string.Empty;
    public int MedicalOptionCategoryId { get; set; }
    public decimal? SalaryBracketMin { get; set; }
    public decimal? SalaryBracketMax { get; set; }
    public decimal? MonthlyRiskContributionPrincipal { get; set; }
    public decimal? MonthlyRiskContributionAdult { get; set; }
    public decimal? MonthlyRiskContributionChild { get; set; }
    public decimal? MonthlyRiskContributionChild2 { get; set; }
    public decimal? MonthlyMsaContributionPrincipal { get; set; }
    public decimal? MonthlyMsaContributionAdult { get; set; }
    public decimal? MonthlyMsaContributionChild { get; set; }
    public decimal? TotalMonthlyContributionsPrincipal { get; set; }
    public decimal TotalMonthlyContributionsAdult { get; set; }
    public decimal TotalMonthlyContributionsChild { get; set; }
    public decimal? TotalMonthlyContributionsChild2 { get; set; }
  }
}