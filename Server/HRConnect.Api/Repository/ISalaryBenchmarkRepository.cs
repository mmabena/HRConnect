namespace HRConnect.Api.Repository
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models.Benchmarking;

  public interface ISalaryBenchmarkRepository
  {
    Task<SalaryBenchmark> CreateAsync(SalaryBenchmark benchmark);
    Task<IEnumerable<SalaryBenchmark>> GetAllAsync();
    Task<SalaryBenchmark> GetByIdAsync(int Id);
    Task<SalaryBenchmark> UpdateAsync(SalaryBenchmark benchmark);
  }
}