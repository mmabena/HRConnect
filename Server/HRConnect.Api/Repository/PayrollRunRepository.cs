namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Data;
  // using HRConnect.Api.Mappers.Payroll;
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
      DateTime dateTime = new DateTime(
        DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

      var payrun = await _context.PayrollRuns.FirstOrDefaultAsync(
        p => p.PeriodDate == dateTime)!;
      return payrun;
    }
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      //Update the current run to be marked as Finalised 
      _context.PayrollRuns.Update(payrollRun);
      await _context.SaveChangesAsync();
    }
  }
}