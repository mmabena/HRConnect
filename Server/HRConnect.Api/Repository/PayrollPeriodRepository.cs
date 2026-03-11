namespace HRConnect.Api.Repository
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Mappers.Payroll;
  using HRConnect.Api.Data;
 using HRConnect.Api.Models.Payroll;

  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class PayrollPeriodRepository : IPayrollPeriodRepository
  {
    private readonly ApplicationDBContext _context;
    public PayrollPeriodRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<PayrollPeriodDto?> GetByIdAsync(Guid id)
    {
      var period = await _context.PayrollPeriods.FirstOrDefaultAsync(p => p.PayrollPeriodId == id);
      if (period == null) //this should never be the case because there will always be a period
        return null;
      return period.ToPayrollPeriodDto();
    }
    public async Task<PayrollPeriod> GetActivePeriod(DateTime dateTime)
    { throw new NotImplementedException(); }
    public async Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod)
    {
      await _context.PayrollPeriods.AddAsync(payrollPeriod);
      await _context.SaveChangesAsync();
      return payrollPeriod.ToPayrollPeriodDto();
    }

    public async Task<IEnumerable<PayrollPeriod>> GetAllPayrollPeriod()
    {
      return await _context.PayrollPeriods.ToListAsync();
    }
  }
}