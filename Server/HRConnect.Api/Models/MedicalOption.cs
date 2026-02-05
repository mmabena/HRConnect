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
    [ForeignKey(nameof(Category))]
    public int MedicalOptionCategoryId { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal SalaryBracketMin { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal SalaryBracketMax { get; set; }
    // Total Monthly Contributions
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal PrincipalAmount { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal Child1Amount { get; set; }
    [Column(TypeName = "decimal(15, 2)")] 
    public decimal? Child2Amount { get; set; }
    // Navigation property to MedicalOptionCategory 1:1 relationship
    public MedicalOptionCategory Category { get; set; } = null!;
  }
}
