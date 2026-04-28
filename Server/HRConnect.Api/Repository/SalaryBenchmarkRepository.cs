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
          .Include(b => b.JobGrade)
          .FirstAsync(b => b.Id == benchmark.Id);

      return created;
    }

    public async Task<IEnumerable<SalaryBenchmark>> GetAllAsync()
    {
      return await _context.SalaryBenchmarks
      .Include(b => b.JobGrade)
      .OrderByDescending(b => b.CreatedDate)
      .ToListAsync();
    }

    public async Task<SalaryBenchmark> GetByIdAsync(int Id)
    {
      return await _context.SalaryBenchmarks
      .Include(b => b.JobGrade)
      .FirstOrDefaultAsync(b => b.Id == Id);
    }

    public async Task<SalaryBenchmark> UpdateAsync(SalaryBenchmark benchmark)
    {
      _context.SalaryBenchmarks.Update(benchmark);
      await _context.SaveChangesAsync();
      return benchmark;
    }
  }
}