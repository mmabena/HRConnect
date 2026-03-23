namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using Microsoft.EntityFrameworkCore;

  public class PensionDeductionRepository(ApplicationDBContext context) : IPensionDeductionRepository
  {
    private readonly ApplicationDBContext _context = context;
    public async Task<PensionDeduction> AddAsync(PensionDeduction pensionDeduction)
    {
      _ = await _context.PensionDeductions.AddAsync(pensionDeduction);
      _ = await _context.SaveChangesAsync();
      return pensionDeduction;
    }

    public Task<bool> DeleteAsync()
    {
      throw new NotImplementedException();
    }

    public async Task<List<PensionDeduction>> GetAllAsync()
    {
      return await _context.PensionDeductions.ToListAsync();
    }

    public async Task<PensionDeduction?> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      PensionDeduction? existingPensionDeduction = await _context.PensionDeductions
        .FirstOrDefaultAsync(pd => pd.EmployeeId == employeeId && !pd.IsLocked);

      return existingPensionDeduction;
    }

    public async Task<PensionDeduction?> GetByEmployeeIdAndLastRunIdAsync(string employeeId, int payRollRunId)
    {
      PensionDeduction? existingPensionDeduction = await _context.PensionDeductions
        .FirstOrDefaultAsync(pd => pd.EmployeeId == employeeId && pd.PayrollRunId == payRollRunId && !pd.IsLocked);

      return existingPensionDeduction;
    }

    public async Task<PensionDeduction?> GetByEmployeeIdAsync(string employeeId)
    {
      PensionDeduction? existingPensionDeduction = await _context.PensionDeductions
        .FirstOrDefaultAsync(pd => pd.EmployeeId == employeeId);

      return existingPensionDeduction ?? null;
    }

    public async Task<List<PensionDeduction>> GetByPayRollRunIdAsync(int payrollRunId)
    {
      return await _context.PensionDeductions.Where(pd => pd.PayrollRunId == payrollRunId).ToListAsync();
    }



    public async Task<PensionDeduction> UpdateAsync(PensionDeduction pensionDeduction)
    {
      _ = _context.PensionDeductions.Update(pensionDeduction);
      _ = await _context.SaveChangesAsync();
      return pensionDeduction;
    }
  }
}
