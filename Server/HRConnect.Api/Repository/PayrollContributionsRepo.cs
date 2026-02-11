namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore;

  public class PayrollContributionsRepo : IPayrollContributionsRepo
  {
    private readonly ApplicationDBContext _context;
    public PayrollContributionsRepo(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<PayrollDeduction> AddDeductionsAsync(PayrollDeduction payrollDeductions)
    {
      _ = await _context.PayrollDeductions.AddAsync(payrollDeductions);
      _ = await _context.SaveChangesAsync();
      return payrollDeductions;
    }
    public async Task<List<PayrollDeduction>> GetAllDeductionsAsync()
    {
      return await _context.PayrollDeductions.ToListAsync();
    }

    public async Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(int employeeId)
    {
      return await _context.PayrollDeductions.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);
    }

  }
}