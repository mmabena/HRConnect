namespace HRConnect.Api.Models
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

  public class Company
  {
    [Required]
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