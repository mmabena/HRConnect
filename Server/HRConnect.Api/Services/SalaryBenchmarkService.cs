namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Net;
  using System.Security.Cryptography.X509Certificates;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Benchmarking;
  using HRConnect.Api.Models.Benchmarking;
  using HRConnect.Api.Repository;
  using HRConnect.Api.Interfaces;

  public class SalaryBenchmarkService : ISalaryBenchmarkService
  {
    private readonly ISalaryBenchmarkRepository _repository;

    public SalaryBenchmarkService(ISalaryBenchmarkRepository repository)
    {
      _repository = repository;
    }

    public async Task<SalaryBenchmarkResponseDto> CreateAsync(SalaryBenchmarkRequestDto request, string createdBy)
    {
      var benchmark = new SalaryBenchmark
      {
        InternalJobGradeId = request.InternalJobGradeId,
        Location = request.Location,
        Salary25th = request.Salary25th,
        Salary50th = request.Salary50th,
        Salary75th = request.Salary75th,
        Source = request.Source,
        CreatedBy = createdBy,
        CreatedDate = DateTime.UtcNow
      };

      var created = await _repository.CreateAsync(benchmark);
      return MapToResponse(created);
    }

    public async Task<IEnumerable<SalaryBenchmarkResponseDto>> GetAllAsync()
    {
      var benchmarks = await _repository.GetAllAsync();
      return benchmarks.Select(MapToResponse);
    }

    private static SalaryBenchmarkResponseDto MapToResponse(SalaryBenchmark benchmark) => new()
    {
      Id = benchmark.Id,
      InternalJobGradeId = benchmark.InternalJobGradeId,
      JobGradeName = benchmark.JobGrade.Name,
      Location = benchmark.Location,
      Salary25th = benchmark.Salary25th,
      Salary50th = benchmark.Salary50th,
      Salary75th = benchmark.Salary75th,
      Source = benchmark.Source,
      CreatedBy = benchmark.CreatedBy,
      CreatedDate = benchmark.CreatedDate
    };
  }
}