namespace HRConnect.Api.DTOs.Benchmarking
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;


  public class EmployeeSalaryBenchmarkDto
  {
    public string EmployeeId { get; set; }
    public string? FullName { get; set; }
    public string PositionTitle { get; set; }
    public decimal MonthlySalary { get; set; }

    public decimal? Salary25th { get; set; }
    public decimal? Salary50th { get; set; }
    public decimal? Salary75th { get; set; }
    public string? Location { get; set; }
    public string? Source { get; set; }
    public int? Year {get; set;}
  }
}