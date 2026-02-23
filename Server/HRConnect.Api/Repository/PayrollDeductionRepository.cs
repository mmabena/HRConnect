namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class PayrollDeductionsRepository : IPayrollDeductionsRepository
  {
    private readonly ApplicationDBContext _context;
    public PayrollDeductionsRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<PayrollDeduction> AddDeductionsAsync(PayrollDeduction payrollDeductions)
    {
      await _context.PayrollDeductions.AddAsync(payrollDeductions);
      await _context.SaveChangesAsync();
      return payrollDeductions;
    }
    public async Task<IEnumerable<PayrollDeduction>> GetAllDeductionsAsync()
    {
      return await _context.PayrollDeductions.ToListAsync();
    }

    public async Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId)
    {
      return await _context.PayrollDeductions.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);
    }

  }
}