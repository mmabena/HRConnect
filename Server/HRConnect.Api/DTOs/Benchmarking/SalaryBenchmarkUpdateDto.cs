namespace HRConnect.Api.DTOs.Benchmarking
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class SalaryBenchmarkUpdateDto
  {
    public decimal Salary25th { get; set; }
    public decimal Salary50th { get; set; }
    public decimal Salary75th { get; set; }
    public string Source { get; set; } = string.Empty;
  }
}