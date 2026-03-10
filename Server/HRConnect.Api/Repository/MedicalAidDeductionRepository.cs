namespace HRConnect.Api.Repository
{
  using Data;
  using Interfaces;
  using Microsoft.EntityFrameworkCore;
  using Models.PayrollContribution;

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
    public async Task<List<MedicalAidDeduction>> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId)
    {
      return await _context.MedicalAidDeductions
        .Where(o => o.EmployeeId == employeeId) 
        .ToListAsync();
    }

    public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync()
    {
      return await _context.MedicalAidDeductions //It can be trimmed down to omit IDs
        .ToListAsync();
    }

    public async Task AddNewMedicalAidDeductionsAsync(string employeeId) // the other details will be in the payload body
    {
      throw new NotImplementedException();
    }

    public async Task UpdateDeductionByEmpIdAsync(string employeeId)
    {
      throw new NotImplementedException();
    }
  }
}