namespace HRConnect.Api.DTOs.Benchmarking
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;


  public class BenchmarkSummaryDto
  {
    public int TotalBenchmarks {get; set; }
    public int TotalPositions { get; set; }
    public int Locations { get; set; }

  }
}