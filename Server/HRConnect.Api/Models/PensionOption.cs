
namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;

  public class PensionOption
  {
    [Key]
    public int PensionOptionId { get; set; }

    public decimal ContributionPercentage { get; set; }

    public ICollection<Employee> Employees { get; set; }

  }
}
