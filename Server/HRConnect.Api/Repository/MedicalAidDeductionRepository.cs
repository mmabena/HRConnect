namespace HRConnect.Api.Repository
{
  using Data;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Models.PayrollDeduction;

  public class MedicalAidDeductionRepository : IMedicalAidDeductionRepository
  {
    private readonly ApplicationDBContext _context;

    /// <summary>
    /// Initializes a new instance of the MedicalOptionRepository class.
    /// </summary>
    /// <param name="context">The database context for medical option operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <remarks>
    /// The repository requires an active ApplicationDBContext instance for all database operations.
    /// The context should be properly configured with the medical options and categories tables.
    /// </remarks>
    public MedicalAidDeductionRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<List<MedicalAidDeduction>> GetMedicalAidDeductionsByEmployeeIdAsync(
      string employeeId)
    {
      return await _context.MedicalAidDeductions
        .AsNoTracking()
        .Include(p => p.PayrollRun)
        .Where(p => p.EmployeeId == employeeId && p.PayrollRun.PayrollRunId != null && p.PayrollRun.IsFinalised == false && p.PayrollRun.IsLocked == false)
        .ToListAsync();
    }

    public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync()
    {
      return await _context.MedicalAidDeductions
        .AsNoTracking()
        .Include(p => p.PayrollRun)
        .Where(p => p.Id != null && p.PayrollRun.PayrollRunId != null && p.PayrollRun.IsFinalised == true && p.PayrollRun.IsLocked == true)
        .ToListAsync();
    }

    public async Task AddNewMedicalAidDeductionsAsync(MedicalAidDeduction deduction)
    {
      await _context.MedicalAidDeductions.AddAsync(deduction);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateDeductionsByEmpIdAsync(string employeeId, int payrollRunId,
      MedicalAidDeduction updatePayloadDeduction)
    {
      var existingDeduction = await _context.MedicalAidDeductions
        .AsNoTracking()
        .Include(p => p.PayrollRun)
        .ThenInclude(q => q.IsFinalised == false && q.IsLocked == false && q.PayrollRunId != null)
        .Where(q => q.EmployeeId == employeeId && q.PayrollRunId == payrollRunId && q.PayrollRun.IsFinalised == false && q.PayrollRun.IsLocked == false)
        .ToListAsync();

      if (existingDeduction == null)
      {
        throw new KeyNotFoundException($"No medical aid deduction found for employee {employeeId} on the active payroll run");
      }

      // Update the existing deduction with new values
      _context.Entry(existingDeduction).CurrentValues.SetValues(updatePayloadDeduction);
      await _context.SaveChangesAsync();
    }
  }
}