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
      //check if a benchmark already exist for the position and location togeher
      bool alreadyExists = await _repository.ExistAsync(request.PositionId, request.Location);
      if (alreadyExists)
      {
        return null;
      }


      var benchmark = new SalaryBenchmark
      {
        PositionId = request.PositionId,
        Location = request.Location,
        Salary25th = request.Salary25th,
        Salary50th = request.Salary50th,
        Salary75th = request.Salary75th,
        Source = request.Source,
        Year = request.Year,
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
      PositionId = benchmark.PositionId,
      PositionTitle = benchmark.Position?.PositionTitle ?? string.Empty,
      JobGradeName = benchmark.Position?.JobGrade?.Name ?? string.Empty,
      Location = benchmark.Location,
      Salary25th = benchmark.Salary25th,
      Salary50th = benchmark.Salary50th,
      Salary75th = benchmark.Salary75th,
      Source = benchmark.Source,
      Year = benchmark.Year,
      CreatedBy = benchmark.CreatedBy,
      CreatedDate = benchmark.CreatedDate
    };

    public async Task<SalaryBenchmarkResponseDto> UpdateAsync(int id, SalaryBenchmarkUpdateDto request)
    {
      var exisiting = await _repository.GetByIdAsync(id);
      bool recordNotFound = exisiting == null;
      if (recordNotFound)
      {
        return null;
      }

      exisiting.Salary25th = request.Salary25th;
      exisiting.Salary50th = request.Salary50th;
      exisiting.Salary75th = request.Salary75th;
      exisiting.Source = request.Source;
      exisiting.Year = request.Year;

      var updated = await _repository.UpdateAsync(exisiting);
      return MapToResponse(updated);
    }

    public async Task<IEnumerable<EmployeeSalaryBenchmarkDto>> GetEmployeeSalaryBenchmarksAsync()
    {
      return await _repository.GetEmployeeSalaryBenchmarksAsync();
    }

    public async Task<BenchmarkSummaryDto> GetSummaryAsync()
    {
      return await _repository.GetSummaryAsync();
    }
  }
}