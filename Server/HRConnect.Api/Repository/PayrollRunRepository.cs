namespace HRConnect.Api.Repository
{
  using Data;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Models.Payroll;
  using Models.PayrollDeduction;

  public class PayrollRunRepository : IPayrollRunRepository
  {
    private readonly ApplicationDBContext _context;
    public PayrollRunRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<PayrollRun> GetAllPayRecordsFromPayRun(PayrollRun payrollRun)
    {
      var runId = payrollRun.PayrollRunId;

      //Get all pension record from current run
      var pensionRecords = await _context.Set<PensionDeduction>()
        .Where(r => r.PayrollRunId == runId)
      .ToListAsync();
      var medicalAidRecords = await _context.Set<MedicalAidDeduction>()
        .Where(r => r.PayrollRunId == runId)
      .ToListAsync();
      payrollRun.Records = pensionRecords.Cast<PayrollRecord>()
                          .Concat(medicalAidRecords.Cast<PayrollRecord>()).ToList();
      return payrollRun;
    }
    public async Task<IEnumerable<PayrollRun>> GetAllPayruns()
    {
      return await _context.PayrollRuns.ToListAsync();
    }
    public async Task<PayrollRun?> GetPayrunByIdAsync(int id)
    {
      var payrun = await _context.PayrollRuns.FirstOrDefaultAsync(p => p.PayrollRunId == id);
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
    public async Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime)
    {
      return await _context.PayrollRuns.FirstOrDefaultAsync(
             p => p.PeriodDate == dateTime);
    }
    /*Get the current payrun using the date and time when this is called*/
    public async Task<PayrollRun?> GetCurrentRunAsync()
    {
      // DateTime dateTime = new DateTime(
      //   DateTime.Now.Year, DateTime.Now.Month, 1);
      // Console.WriteLine($"CURRENT DATE WHEN GETTING RUN {dateTime}");

      // var payrun = await _context.PayrollRuns.FirstOrDefaultAsync(
      //   p => p.PeriodDate.Month == dateTime.Month);
      var payrun = await _context.PayrollRuns.Where(r => !r.IsLocked)
        .OrderByDescending(r => r.PayrollRunId)
        .FirstOrDefaultAsync();
      if (payrun != null)
        Console.WriteLine($"-------CURRENT RUN ID {payrun.PayrollRunId}-------");
      return payrun;
    }

    // public Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord)
    // {
    //   throw new NotImplementedException();
    // }
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      //Update the current run to be marked as Finalised 
      _context.PayrollRuns.Update(payrollRun);
      await _context.SaveChangesAsync();
    }
    public async Task<PayrollRun?> GetLastPayrun()
    {
      return await _context.PayrollRuns.OrderByDescending(r => r.PayrollRunId)
        .FirstOrDefaultAsync();
    }
  }
}