namespace HRConnect.Api.DTOs
{
  public class ResponseEligibileOptionsDto
  {
    //Employee details (Summarized and applicable to medical options query)
    public string EmployeeName { get; set; } //Employee FirstName
    public string EmployeeSurname { get; set; }
    public decimal Salary { get; set; }
    public int NumberOfPrincipals { get; set; } = 1; // Default to 1 principal, as most employees will have at least themselves as a principal
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public int MedicalOptionId { get; set; }
    public string MedicalOptionName { get; set; } = string.Empty;
    public string MedicalOptionCategoryName { get; set; } = string.Empty;
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
    //Contributionstion details (Summarized and applicable to medical options query)
    //Premium BreakDown
    public decimal EstimatedPrincipalMonthlyPremium { get; set; }
    public decimal? EstimatedAdultMonthlyPremium { get; set; }
    public decimal? EstimatedChildMonthlyPremium { get; set; }
    //Total monthly Premium
    public decimal EstimatedTotalMonthlyPremium { get; set; }
  }
}
