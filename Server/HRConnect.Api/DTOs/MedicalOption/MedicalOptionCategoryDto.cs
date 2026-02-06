namespace HRConnect.Api.DTOs.MedicalOption
{

  using HRConnect.Api.Models;

  public class MedicalOptionCategoryDto
  {
    // Fetch all Medical Options grouped by Category, Incl Salary Bracket

    public int MedicalOptionCategoryId { get; set; }

    public string MedicalOptionCategoryName { get; set; } = string.Empty;

    public decimal? MonthlyRiskContributionPrincipal { get; set; }


    public decimal MonthlyRiskContributionAdult { get; set; }

    public decimal MonthlyRiskContributionChild { get; set; }

    public decimal? MonthlyRiskContributionChild2 { get; set; }

    public decimal? MonthlyMsaContributionPrincipal { get; set; }

    public decimal? MonthlyMsaContributionAdult { get; set; }

    public decimal? MonthlyMsaContributionChild { get; set; }

    public decimal? TotalMonthlyContributionsPrincipal { get; set; }

    public decimal TotalMonthlyContributionsAdult { get; set; }

    public decimal TotalMonthlyContributionsChild { get; set; }

    public decimal? TotalMonthlyContributionsChild2 { get; set; }

    public MedicalOption MedicalOption { get; set; }
  }
}
