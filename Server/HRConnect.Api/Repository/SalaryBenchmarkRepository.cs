namespace HRConnect.Api.Repository
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Benchmarking;
  using HRConnect.Api.Models.Benchmarking;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Interfaces;
  using System.Diagnostics;
  using System.Security.Cryptography.X509Certificates;
  using HRConnect.Api.Models;

  public class SalaryBenchmarkRepository : ISalaryBenchmarkRepository
  {
    private readonly ApplicationDBContext _context;

    public SalaryBenchmarkRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<SalaryBenchmark> CreateAsync(SalaryBenchmark benchmark)
    {
      _context.SalaryBenchmarks.Add(benchmark);
      await _context.SaveChangesAsync();

      var created = await _context.SalaryBenchmarks
          .Include(b => b.Position)
          .ThenInclude(p => p.JobGrade)
          .FirstAsync(b => b.Id == benchmark.Id);

      return created;
    }

    public async Task<IEnumerable<SalaryBenchmark>> GetAllAsync()
    {
      int currentYear = DateTime.UtcNow.Year;

      return await _context.SalaryBenchmarks
      .Include(b => b.Position)
      .ThenInclude(p => p.JobGrade)
      .Where(b => b.Year == currentYear)
      .OrderByDescending(b => b.CreatedDate)
      .ToListAsync();
    }

    public async Task<SalaryBenchmark> GetByIdAsync(int Id)
    {
      return await _context.SalaryBenchmarks
      .Include(b => b.Position)
      .ThenInclude(p => p.JobGrade)
      .FirstOrDefaultAsync(b => b.Id == Id);
    }

    public async Task<SalaryBenchmark> UpdateAsync(SalaryBenchmark benchmark)
    {
      _context.SalaryBenchmarks.Update(benchmark);
      await _context.SaveChangesAsync();
      return benchmark;
    }

    public async Task<IEnumerable<EmployeeSalaryBenchmarkDto>> GetEmployeeSalaryBenchmarksAsync()
    {
      var employee = await _context.Employees
      .Where(e => e.IsActive)
      .Include(e => e.Position)
      .OrderBy(e => e.Position.PositionTitle)
      .ThenBy(e => e.Surname).ToListAsync();

      var benchmarks = await _context.SalaryBenchmarks
      .Where(b => b.Year == DateTime.UtcNow.Year).ToListAsync();

    var benchmarkLookup = benchmarks
        .GroupBy(b => new { b.PositionId, b.Location })
        .ToDictionary(
            group => group.Key,
            group => group.OrderByDescending(b => b.CreatedDate).First()
        );

      return employee.Select(e =>
      {
        string employeeBranch = e.Branch switch
        {
          Branch.Johannesburg => "Johannesburg",
          Branch.CapeTown => "Cape Town",
          _ => e.Branch.ToString()
        };

         benchmarkLookup.TryGetValue(
            new { e.PositionId, Location = employeeBranch },
            out var benchmark
        );


        Console.WriteLine($"Employee: {e.Name}, Branch: {employeeBranch}, Benchmark location: {benchmark?.Location ?? "none"}, Match: {benchmark?.Location == employeeBranch}");

        if (benchmark != null && benchmark.Location != employeeBranch)
        {
          benchmark = null;
        }

        return new EmployeeSalaryBenchmarkDto
        {
          EmployeeId = e.EmployeeId,
          FullName = e.Name + " " + e.Surname,
          PositionTitle = e.Position?.PositionTitle,
          MonthlySalary = e.MonthlySalary,
          Salary25th = benchmark?.Salary25th,
          Salary50th = benchmark?.Salary50th,
          Salary75th = benchmark?.Salary75th,
          Location = benchmark?.Location,
          Source = benchmark?.Source,
          Year = benchmark?.Year,
        };
      });
    }

    public async Task<BenchmarkSummaryDto> GetSummaryAsync()
    {
      var benchmarks = await _context.SalaryBenchmarks.ToListAsync();

      return new BenchmarkSummaryDto
      {
        TotalBenchmarks = benchmarks.Count,
        TotalPositions = benchmarks.Select(b => b.PositionId).Distinct().Count(),
        Locations = benchmarks.Select(b => b.Location).Distinct().Count()
      };
    }

    public async Task<bool> ExistAsync(int positionId, string location)
    {
      return await _context.SalaryBenchmarks
      .AnyAsync(b => b.PositionId == positionId && b.Location == location);
    }

    public async Task ArchiveOldBenchmarksAsync()
    {
      int currentYear = DateTime.UtcNow.Year;

      var oldBenchmarks = await _context.SalaryBenchmarks
      .Where(b => !b.IsArchived && b.Year < currentYear)
      .ToListAsync();

      if (oldBenchmarks == null)
      {
        return;
      }

      foreach (var benchmark in oldBenchmarks)
      {
        benchmark.IsArchived = true;
        benchmark.ArchivedDate = DateTime.UtcNow;
      }

      await _context.SaveChangesAsync();

      //making sure it outputs what i want
      Console.WriteLine($"[BenchmarkArchiveService] Archived {oldBenchmarks.Count} benchmark(s) on {DateTime.UtcNow:yyyy-MM-dd}");
    }
  }
}