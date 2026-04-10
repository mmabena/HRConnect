namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll.Earnings;
  using Microsoft.EntityFrameworkCore;

  public class PayrollEarningRepository(ApplicationDBContext context) : IPayrollEarningRepository
  {
    private readonly ApplicationDBContext _context = context;

    public async Task<PayrollEarning> AddAsync(PayrollEarning payrollEarning)
    {
      _ = await _context.PayrollEarnings.AddAsync(payrollEarning);
      _ = await _context.SaveChangesAsync();
      return payrollEarning;
    }

    public async Task<List<PayrollEarning>> GetAllAsync()
    {
      return await _context.PayrollEarnings.ToListAsync();
    }

    public async Task<PayrollEarning?> GetByPayrollEarningId(string payrollEarningId)
    {
      return await _context.PayrollEarnings.Where(pre => pre.PayrollEarningId == payrollEarningId).FirstOrDefaultAsync();
    }

    public async Task<List<PayrollEarning>> GetByTaxCode(int taxCode)
    {
      return await _context.PayrollEarnings.Where(pre => pre.TaxCode == taxCode).ToListAsync();
    }

    public async Task<PayrollEarning> UpdateAsync(PayrollEarning payrollEarning)
    {
      _ = _context.PayrollEarnings.Update(payrollEarning);
      _ = await _context.SaveChangesAsync();
      return payrollEarning;
    }
  }
}
