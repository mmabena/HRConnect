
namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations.Schema;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;

  using System.ComponentModel.DataAnnotations;

  public class PensionOption
  {
    [Key]
    public int PensionOptionId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal ContributionPercentage { get; set; }
    public ICollection<Employee> Employees { get; set; }
        public ICollection<EmployeePensionEnrollment> EmployeePensionEnrollment { get; set; } = [];
        public ICollection<PensionDeduction> PensionDeduction { get; set; } = [];
  }
}