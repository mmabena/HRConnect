namespace HRConnect.Api.DTOs.MedicalOption
{
  using System.Text.Json.Serialization;
  using HRConnect.Api.Models;

  public class MedicalOptionCategoryDto
  {
    // Fetch all Medical Options grouped by Category, Incl Salary Bracket
    // TODO: Use JsonPropertyName to override the property name in the JSON response
    [JsonPropertyName("optionCategoryId")]
    public int MedicalOptionCategoryId { get; set; }
    [JsonPropertyName("OptionCategoryName")]
    public string MedicalOptionCategoryName { get; set; } = string.Empty;

    public decimal? MonthlyRiskContributionPrincipal { get; set; }

    public decimal MonthlyRiskContributionAdult { get; set; }

    public decimal MonthlyRiskContributionChild { get; set; }

    public decimal? MonthlyRiskContributionChild2 { get; set; }
    [JsonPropertyName("PrincipalMonthlyMedicalSavingsContributions")]
    public decimal? MonthlyMsaContributionPrincipal { get; set; }
    [JsonPropertyName("AdultMonthlyMedicalSavingsContributions")]
    public decimal? MonthlyMsaContributionAdult { get; set; }
    [JsonPropertyName("ChildMonthlyMedicalSavingsContributions")]
    public decimal? MonthlyMsaContributionChild { get; set; }

    public decimal? TotalMonthlyContributionsPrincipal { get; set; }

    public decimal TotalMonthlyContributionsAdult { get; set; }
    
    public decimal TotalMonthlyContributionsChild { get; set; }

    public decimal? TotalMonthlyContributionsChild2 { get; set; }

    // Navigation property to MedicalOption 1:1 relationship - 
    // Where the grouped list of options/variants under this category is fetched
    public List<MedicalOptionDto> Options { get; set; } = new();
  }
}