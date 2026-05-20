namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models.Payroll;
  using Microsoft.EntityFrameworkCore;

  public class PayrollRunRepository : IPayrollRunRepository
  {
    private readonly ApplicationDBContext _context;

    public PayrollRunRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<PayrollRun>> GetAllPayruns()
    {
      return await _context.PayrollRuns.ToListAsync();
    }
    public async Task<PayrollRun?> GetUnlockedPayrunByRunNumberAsync(int payrollRunNumber)
    {

      var payrun = await _context.PayrollRuns.Include(r => r.Records).FirstOrDefaultAsync(p => p.PayrollRunNumber == payrollRunNumber);
      return payrun;
    }
    public async Task<PayrollRun?> GetPayrunByRunNumberAsync(int payrollRunNumber)
    {
      var payrun = await _context.PayrollRuns.Where(r => !r.IsLocked).Include(r => r.Records).FirstOrDefaultAsync(p => p.PayrollRunNumber == payrollRunNumber);
      return payrun;
    }
    public async Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun)
    {
      await _context.PayrollRuns.AddAsync(payrollRun);
      await _context.SaveChangesAsync();
      return payrollRun;
    }

    public async Task<PayrollRun?> GetRunByDateAsync(int payrollRunNumber, DateTime startDate, DateTime endDate)
    {
      var run = await _context.PayrollRuns
      .Where(r =>
          (r.PayrollRunNumber == payrollRunNumber) &&
          (r.Period.EndDate >= startDate) &&
          (r.Period.StartDate <= endDate))
        .Include(r => r.Period)
        .Include(r => r.Records)
        .AsSplitQuery()
        .FirstOrDefaultAsync();


      if (run == null)
      {
        return null;
      }
      return run;
    }
    public async Task<PayrollRun?> GetCurrentRunAsync()
    {
      var payrun = await _context.PayrollRuns.Where(r => !r.IsLocked)
        .OrderByDescending(r => r.PayrollRunNumber)
        .FirstOrDefaultAsync();
      if (payrun != null)
        return payrun;
      return null;
    }

    public Task UpdateRun(PayrollRun payrollRun)
    {
      //Update the current run to be marked as Finalised
      _context.PayrollRuns.Update(payrollRun);

      return Task.CompletedTask;
    }
    public async Task<PayrollRun?> GetLastPayrun()
    {
      return await _context.PayrollRuns.OrderByDescending(r => r.PayrollRunNumber)
        .FirstOrDefaultAsync();
    }
    public async Task UpdateExpiredRun(PayrollRun payrollRun)
    {
      _context.PayrollRuns.Update(payrollRun);
      await _context.SaveChangesAsync();
    }
    public async Task<PayrollRun?> IsExpiredPayRunUnlocked()
    {
      var unlockedRun = await _context.PayrollRuns.Where(r => !r.IsLocked)
                   .OrderByDescending(r => r.PayrollRunNumber)
                   .FirstOrDefaultAsync();
      if (unlockedRun != null)
        return unlockedRun;
      return null;
    }
  }
}