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
    //Navigation property to MedicalOption
    public ICollection<MedicalOption> MedicalOptions { get; set; } = new List<MedicalOption>();
  }
}
