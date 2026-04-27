namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs.Benchmarking;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;


  public interface ISalaryBenchmarkService
  {
    Task<SalaryBenchmarkResponseDto> CreateAsync(SalaryBenchmarkRequestDto request, string createdBy);
    Task<IEnumerable<SalaryBenchmarkResponseDto>> GetAllAsync();
  }
}