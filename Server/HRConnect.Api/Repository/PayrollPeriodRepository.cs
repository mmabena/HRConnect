namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;
  using SQLitePCL;

  public class PayrollPeriodRepository : IPayrollPeriodRepository
  {
    private readonly ApplicationDBContext _context;
    public PayrollPeriodRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<PayrollPeriodDto?> GetByIdAsync(Guid id)
    {
      return await _context.PayrollPeriods.FirstOrDefaultAsync(p => p.PayrollPeriodId == id);
    }
    public async Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime)
    { }
    public async Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod)
    { }
  }
}