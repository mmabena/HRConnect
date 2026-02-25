namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  public class StatutoryContributionType
  {
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;  //'UIF' or 'SDL'
    public string Name { get; set; } = string.Empty;  //'Unemployment Insurance Fund'
    [Precision(18, 4)]
    public decimal EmployeeRate { get; set; }         //0.01 (1%)
    [Precision(18, 4)]
    public decimal EmployerRate { get; set; }         //0.01 (1%)
    [Precision(18, 4)]
    public decimal? CapAmount { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
  }
}