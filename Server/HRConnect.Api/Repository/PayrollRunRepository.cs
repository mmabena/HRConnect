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

    /*
     * Remove ability to query the payrollRun to get a particular record
     *TPC Inheritance strategy allows for concrete classes to be queried
     * */
    public Task<PayrollRun> GetAllPayRecordsFromPayRun(PayrollRun payrollRun)
    {
      // //Get all pension record from current run
      // var pensionRecords = await _context.Set<PensionDeduction>()
      //   .Where(r => r.PayrollRunId == payrollRun.payrollRunNumber)
      // .ToListAsync();
      // var medicalAidRecords = await _context.Set<MedicalAidDeduction>()
      //   .Where(r => r.PayrollRunId == payrollRun.payrollRunNumber)
      // .ToListAsync();
      // payrollRun.Records = pensionRecords.Cast<PayrollRecord>()
      //                     .Concat(medicalAidRecords.Cast<PayrollRecord>()).ToList();
      throw new NotImplementedException();
      // return payrollRun;
    }
    public async Task<IEnumerable<PayrollRun>> GetAllPayruns()
    {
      return await _context.PayrollRuns.ToListAsync();
    }
    public async Task<PayrollRun?> GetPayrunByRunNumberAsync(int id)
    {
      var payrun = await _context.PayrollRuns.Where(r => !r.IsLocked).Include(r => r.Records).FirstOrDefaultAsync(p => p.PayrollRunNumber == id);
      return payrun;
    }
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    public async Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun)
    {
      await _context.PayrollRuns.AddAsync(payrollRun);
      await _context.SaveChangesAsync();
      return payrollRun;
    }
    /*Get the current payrun using the provided date*/
    public async Task<PayrollRun?> GetRunByDateAsync(int payrollRunNumber, DateTime startDate, DateTime endDate)
    {
      var run = await _context.PayrollRuns.Include(r => r.Period).Where(r =>
          (r.PayrollRunNumber == payrollRunNumber) &&
          (r.Period.StartDate >= startDate) &&
          (r.Period.StartDate <= endDate))
        .Include(r => r.Records)//I do not know if this is necessary 
        .FirstOrDefaultAsync();


      if (run == null)
      {
        return null;
      }
      return run;
    }
    /*Get the current payrun using the date and time when this is called*/
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