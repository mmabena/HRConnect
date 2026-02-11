
namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  //This is just a seeded Employee
  public class Employee
  {
    [Key]
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    [Precision(18, 2)]
    public decimal MonthlySalary { get; set; }
  }
}