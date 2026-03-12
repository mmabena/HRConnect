namespace HRConnect.Api.Repository
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Mappers.Payroll;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Quartz.Xml.JobSchedulingData20;

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

    public async Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime)
    {
      return await _context.PayrollPeriods.FirstOrDefaultAsync(
         p => p.StartDate <= dateTime &&
         p.EndDate >= dateTime);
    }
    // Get The Last Period
    public async Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod)
    {
      await _context.PayrollPeriods.AddAsync(payrollPeriod);
      await _context.SaveChangesAsync();
      return payrollPeriod.ToPayrollPeriodDto();
    }

    public async Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriod()
    {
      return await _context.PayrollPeriods.Include(p => p.Runs).ThenInclude(r => r.Records).ToListAsync();
    }
    public async Task UpdateAsync(PayrollPeriod payrollPeriod)
    {
      _context.PayrollPeriods.Update(payrollPeriod);
      await _context.SaveChangesAsync();
    }

    public async Task<PayrollPeriod?> GetLastPeriodAsync()
    {
      return await _context.PayrollPeriods.Include(p => p.Runs).ThenInclude(r => r.Records).Where(p => !p.IsLocked)
        .OrderByDescending(p => p.PayrollPeriodId)
      .FirstOrDefaultAsync();
    }

    public async Task<PayrollPeriod?> GetLastPeriodForRollOver()
    {
      return await _context.PayrollPeriods.Include(p => p.Runs).AsNoTracking().Where(p => !p.IsLocked)
      .FirstOrDefaultAsync();
    }
  }
}