namespace HRConnect.Api.DTOs.Benchmarking
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;


  public class SalaryBenchmarkResponseDto
  {
    public int Id { get; set; }
    public string JobGradeName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Salary25th { get; set; }
    public decimal Salary50th { get; set; }
    public decimal Salary75th { get; set; }
    public string Source { get; set; } = string.Empty;
    public int InternalJobGradeId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
  }
}