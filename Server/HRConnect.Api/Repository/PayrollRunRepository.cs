namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Data;
  using HRConnect.Api.Mappers.Payroll;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;


  public class PayrollRunRepository : IPayrollRunRepository
  {
    private readonly ApplicationDBContext _context;
    public PayrollRunRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<PayrollRunDto> GetByIdAsync(int id)
    {
      throw new NotImplementedException();
    }
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    public async Task<PayrollRunDto> CreatePayrollRunAsync(PayrollRun payrollRun)
    {
      throw new NotImplementedException();
    }
    public async Task<bool> HasFinalRunAsync(int id)
    {
      throw new NotImplementedException();
    }
    public async Task<PayrollRun?> GetCurrentRunAsync()
    {
      throw new NotImplementedException();
    }
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      //Update the current run to be marked as Finalised 
      _context.PayrollRuns.Update(payrollRun);
      await _context.SaveChangesAsync();
    }
  }
}