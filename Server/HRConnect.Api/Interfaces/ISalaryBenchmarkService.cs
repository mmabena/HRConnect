namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.Benchmarking;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Cryptography;
  using System.Threading.Tasks;


  public interface ISalaryBenchmarkService
  {
    Task<SalaryBenchmarkResponseDto> CreateAsync(SalaryBenchmarkRequestDto request, string createdBy);
    Task<IEnumerable<SalaryBenchmarkResponseDto>> GetAllAsync();
    Task<SalaryBenchmarkResponseDto> UpdateAsync(int id, SalaryBenchmarkUpdateDto request);
    Task<IEnumerable<EmployeeSalaryBenchmarkDto>> GetEmployeeSalaryBenchmarksAsync();
    Task<BenchmarkSummaryDto> GetSummaryAsync();
  }
}