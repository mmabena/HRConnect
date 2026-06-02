namespace HRConnect.Api.Models
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;

  public class Company
  {
    [Key]
    public string CompanyId { get; set; } = string.Empty;
    [Required]
    public string CompanyName { get; set; } = string.Empty;
    [Required]
    [StringLength(14)]
    public string RegistrationNumber { get; set; } = string.Empty;
    [Required]
    [StringLength(10)]
    public string UIFNumber { get; set; } = string.Empty;
    public string? VATNumber { get; set; } = string.Empty;
    [Required]
    [StringLength(10)]
    public string ContactNumber { get; set; } = string.Empty;
    [Required]
    public string CompanyAddress { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
  }
}