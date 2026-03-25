namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class MedicalOption
  {
    [Key]
    [Column("MedicalOptionId")]
    public int MedicalOptionId { get; set; }
    public string MedicalOptionName { get; set; } = string.Empty;
    [ForeignKey(nameof(MedicalOptionCategory))]
    public int MedicalOptionCategoryId { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? SalaryBracketMin { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? SalaryBracketMax { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyRiskContributionPrincipal { get; set; }
    [Required]
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyRiskContributionAdult { get; set; }
    [Required]
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyRiskContributionChild { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyRiskContributionChild2 { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? MonthlyMsaContributionPrincipal { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyMsaContributionAdult { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyMsaContributionChild { get; set; }
    
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalMonthlyContributionsPrincipal { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalMonthlyContributionsAdult { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalMonthlyContributionsChild { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalMonthlyContributionsChild2 { get; set; }
    
    // Navigation property to MedicalOptionCategory *:1 relationship
    public MedicalOptionCategory MedicalOptionCategory { get; set; } = null!;
  }
}