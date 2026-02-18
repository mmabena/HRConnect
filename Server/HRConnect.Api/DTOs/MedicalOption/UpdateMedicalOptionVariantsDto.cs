namespace HRConnect.Api.DTOs.MedicalOption
{
  public class UpdateMedicalOptionVariantsDto
  {
    public int MedicalOptionId { get; set; }
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