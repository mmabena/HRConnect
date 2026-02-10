
namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using Microsoft.EntityFrameworkCore;

  //This is just a seeded Employee
  public class Employee
  {
    [Key]
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    [Precision(9, 2)]
    public decimal MonthlySalary { get; set; }
  }
}