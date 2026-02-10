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
    public async Task<PayrollDeduction> AddUifDeductionAsync(decimal uifEmployee, decimal uifEmployer, decimal sdlAmount, int employeeId)
    {
      PayrollDeduction deduction = new()
      {
        // SdlAmount = sdlAmount,
        UifEmployeeAmount = uifEmployee,
        UifEmployerAmount = uifEmployer,
        EmployeeId = employeeId
      };

      _ = await _context.PayrollDeductions.AddAsync(deduction);
      _ = await _context.SaveChangesAsync();
      return deduction;
    }
    public async Task<List<PayrollDeduction>> GetAllUifDeductionsAsync()
    {
      return await _context.PayrollDeductions.ToListAsync();
    }
    public async Task<PayrollDeduction?> GetUifDeductionsByIdAsync(int id)
    {
      return await _context.PayrollDeductions
      .FirstOrDefaultAsync(p => p.PayrollDeductionId == id);
    }
    public async Task<PayrollDeduction?> GetUifDeductionsByEmployeeIdAsync(int employeeId)
    {
      return await _context.PayrollDeductions.FirstOrDefaultAsync(p => p.EmployeeId == employeeId);
    }

  }
}