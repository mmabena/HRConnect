namespace HRConnect.Api.Models.Benchmarking
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  public class SalaryBenchmark
  {
    public int Id { get; set; }
    public int PositionId { get; set; }
    public Position? Position { get; set; }
    public decimal Salary25th { get; set; }
    public decimal Salary50th { get; set; }
    public decimal Salary75th { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
  }
}