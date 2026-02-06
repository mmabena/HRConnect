namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class MedicalOptionCategory
  {
    [Key]
    [Column("MedicalOptionCategoryId")]
    public int MedicalOptionCategoryId { get; set; }
    [Required]
    public string MedicalOptionCategoryName { get; set; } = string.Empty;
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyRiskContributionPrincipal { get; set; }
    [Required]
    [Column(TypeName = "decimal(15, 2)")]
    public decimal MonthlyRiskContributionAdult { get; set; }
    [Required]
    [Column(TypeName = "decimal(15, 2)")]
    public decimal MonthlyRiskContributionChild { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyRiskContributionChild2 { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? MonthlyMsaContributionPrincipal { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyMsaContributionAdult { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MonthlyMsaContributionChild { get; set; }
    //TODO: Add Total Monthly Contributions field - Done
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalMonthlyContributionsPrincipal { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalMonthlyContributionsAdult { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal TotalMonthlyContributionsChild { get; set; }
    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalMonthlyContributionsChild2 { get; set; }
    [Required]
    public MedicalOption MedicalOption { get; set; } = null!;
  }
}
