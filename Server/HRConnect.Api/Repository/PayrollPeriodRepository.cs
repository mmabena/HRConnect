namespace HRConnect.Api.Repository
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Mappers.Payroll;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using System.Data.Common;

  public class PayrollPeriodRepository : IPayrollPeriodRepository
  {
    private readonly ApplicationDBContext _context;
    public PayrollPeriodRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<PayrollPeriodDto?> GetByIdAsync(int id)
    {
      var period = await _context.PayrollPeriods.Include(p => p.Runs).FirstOrDefaultAsync(p => p.PayrollPeriodId == id);
      if (period == null) //this should never be the case because there will always be a period
        return null;
      return period.ToPayrollPeriodDto();
    }
    /*Active period depends on the financial year. April-March*/

    public async Task<PayrollPeriod?> GetPeriodByDate(DateTime dateTime)
    {
      return await _context.PayrollPeriods.FirstOrDefaultAsync(
         p => p.StartDate <= dateTime &&
         p.EndDate >= dateTime);
    }

    // public async Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime)
    // {
    //   return await _context.PayrollPeriods.FirstOrDefaultAsync(
    //      p => p.StartDate <= dateTime &&
    //      p.EndDate >= dateTime);
    // }

    public async Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod)
    {
      await _context.PayrollPeriods.AddAsync(payrollPeriod);
      await _context.SaveChangesAsync();
      return payrollPeriod.ToPayrollPeriodDto();
    }

    public async Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriod()
    {
      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        var periods = await _context.PayrollPeriods
         .Include(p => p.Runs)
         .ThenInclude(r => r.Records)
         .AsSplitQuery()
         .ToListAsync();

        await transaction.CommitAsync();
        return periods;
      }
      catch (DbException ex)
      {
        await transaction.RollbackAsync();
        Console.WriteLine($"Failed Database Transaction With :{ex}");
        throw;
      }
    }
    public async Task UpdateAsync(PayrollPeriod payrollPeriod)
    {
      _context.PayrollPeriods.Update(payrollPeriod);
      await _context.SaveChangesAsync();
    }


    public async Task<PayrollPeriod?> GetLastPeriodAsync()
    {
      using var transaction = await _context.Database.BeginTransactionAsync();
      try
      {
        var periods = await _context.PayrollPeriods
              .Where(p => !p.IsLocked)//filter out early to prevent hogging up memory usage
              .OrderByDescending(p => p.PayrollPeriodId)
              .Include(p => p.Runs)
              .ThenInclude(r => r.Records)
              .AsSplitQuery() //prevent what is called 'Cartesian Explosion' (we have 3 record types so far to query)
            .FirstOrDefaultAsync();

        await transaction.CommitAsync();
        return periods;
      }
      catch (DbException ex)
      {
        await transaction.RollbackAsync();
        Console.WriteLine($"Failed Database Transaction With :{ex}");
        throw;
      }
    }

    public async Task<PayrollPeriod?> GetLastPeriodForRollOver()
    {
      return await _context.PayrollPeriods.Include(p => p.Runs).AsNoTracking().Where(p => !p.IsLocked)
      .FirstOrDefaultAsync();
    }
  }
}