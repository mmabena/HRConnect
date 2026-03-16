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
        .Include(p => p.PayrollRun)
        .Where(o => o.EmployeeId == employeeId)
        .ToListAsync();
    }

    public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync()
    {
      return await _context.MedicalAidDeductions
        .Where(ma => ma.Id != 0 || ma.Id != null)//It can be trimmed down to omit IDs
        .ToListAsync();
    }

    public async Task AddNewMedicalAidDeductionsAsync(MedicalAidDeduction deduction)
    {
      await _context.MedicalAidDeductions.AddAsync(deduction);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateDeductionByEmpIdAsync(string employeeId, MedicalAidDeduction deduction)
    {
      var existingDeduction = await _context.MedicalAidDeductions
        .FirstOrDefaultAsync(d => d.EmployeeId == employeeId);

      if (existingDeduction == null)
      {
        throw new KeyNotFoundException($"No medical aid deduction found for employee {employeeId}");
      }

      // Update the existing deduction with new values
      _context.Entry(existingDeduction).CurrentValues.SetValues(deduction);
      await _context.SaveChangesAsync();
    }
  }
}