namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.PayrollDeduction;
  using Microsoft.EntityFrameworkCore;
  
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
    /// <summary>
    /// Retrieves all medical aid deductions for a specific employee from active payroll runs.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <returns>A list of medical aid deductions for the employee from non-finalized, non-locked payroll runs.</returns>
    public async Task<List<MedicalAidDeduction>> GetMedicalAidDeductionsByEmployeeIdAsync(
      string employeeId)
    {
      return await _context.MedicalAidDeductions
        .AsNoTracking()
        .Include(p => p.PayrollRun)
        .Where(p => p.EmployeeId == employeeId && p.PayrollRun.PayrollRunId != null && p.PayrollRun.IsFinalised == false && p.PayrollRun.IsLocked == false)
        .ToListAsync();
    }

    /// <summary>
    /// Retrieves all medical aid deductions from finalized and locked payroll runs.
    /// </summary>
    /// <returns>A read-only list of all finalized medical aid deductions.</returns>
    public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync()
    {
      return await _context.MedicalAidDeductions
        .AsNoTracking()
        .Include(p => p.PayrollRun)
        .Where(p => p.Id != null && p.PayrollRun.PayrollRunId != null && p.PayrollRun.IsFinalised == true && p.PayrollRun.IsLocked == true)
        .ToListAsync();
    }

    /// <summary>
    /// Adds a new medical aid deduction record to the database.
    /// </summary>
    /// <param name="deduction">The medical aid deduction entity to add.</param>
    public async Task AddNewMedicalAidDeductionsAsync(MedicalAidDeduction deduction)
    {
      await _context.MedicalAidDeductions.AddAsync(deduction);
      await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing medical aid deduction for a specific employee and payroll run.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="payrollRunId">The identifier of the payroll run.</param>
    /// <param name="updatePayloadDeduction">The updated deduction data.</param>
    /// <exception cref="KeyNotFoundException">Thrown when no active deduction is found for the specified employee and payroll run.</exception>
    public async Task UpdateDeductionsByEmpIdAsync(string employeeId, int payrollRunId,
      MedicalAidDeduction updatePayloadDeduction)
    {
       var existingDeduction = await _context.MedicalAidDeductions
         .Include(d => d.PayrollRun)
         .FirstOrDefaultAsync(d =>
           d.EmployeeId == employeeId &&
           d.PayrollRunId == payrollRunId &&
           d.PayrollRun != null &&
           !d.PayrollRun.IsFinalised &&
           !d.PayrollRun.IsLocked);

      if (existingDeduction == null)
      {
        throw new KeyNotFoundException($"No medical aid deduction found for employee {employeeId} on the active payroll run");
      }
      
      // Update mutable fields only
      existingDeduction.Name = updatePayloadDeduction.Name;
      existingDeduction.Surname = updatePayloadDeduction.Surname;
      existingDeduction.Branch = updatePayloadDeduction.Branch;
      existingDeduction.Salary = updatePayloadDeduction.Salary;
      existingDeduction.EmployeeStartDate = updatePayloadDeduction.EmployeeStartDate;

      existingDeduction.MedicalOptionId = updatePayloadDeduction.MedicalOptionId;
      existingDeduction.OptionName = updatePayloadDeduction.OptionName;
      existingDeduction.MedicalCategoryId = updatePayloadDeduction.MedicalCategoryId;
      existingDeduction.OptionCategoryName = updatePayloadDeduction.OptionCategoryName;

      existingDeduction.PrincipalCount = updatePayloadDeduction.PrincipalCount;
      existingDeduction.AdultCount = updatePayloadDeduction.AdultCount;
      existingDeduction.ChildrenCount = updatePayloadDeduction.ChildrenCount;

      existingDeduction.PrincipalPremium = updatePayloadDeduction.PrincipalPremium;
      existingDeduction.SpousePremium = updatePayloadDeduction.SpousePremium;
      existingDeduction.ChildPremium = updatePayloadDeduction.ChildPremium;
      existingDeduction.TotalDeductionAmount = updatePayloadDeduction.TotalDeductionAmount;

      existingDeduction.EffectiveDate = updatePayloadDeduction.EffectiveDate;
      existingDeduction.TerminationDate = updatePayloadDeduction.TerminationDate;
      existingDeduction.TerminationReason = updatePayloadDeduction.TerminationReason;
      existingDeduction.IsActive = updatePayloadDeduction.IsActive;
      existingDeduction.UpdatedDate = updatePayloadDeduction.UpdatedDate == default
        ? DateTime.Now.ToLocalTime()
        : updatePayloadDeduction.UpdatedDate;

      await _context.SaveChangesAsync();
      
    }

    /// <summary>
    /// Retrieves the currently active medical aid deduction for a specific employee.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <returns>The active medical aid deduction or null if not found.</returns>
    public async Task<MedicalAidDeduction?> GetActiveMedicalAidDeductionByEmpIdAsync(string employeeId)
    {
      return await _context.MedicalAidDeductions
        .Include(d => d.PayrollRun)
        .Where(d =>
          d.EmployeeId == employeeId &&
          d.IsActive &&
          d.TerminationDate == null &&
          d.PayrollRun != null &&
          !d.PayrollRun.IsFinalised &&
          !d.PayrollRun.IsLocked)
        .OrderByDescending(d => d.CreatedDate)
        .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Updates a medical aid deduction record with termination details.
    /// </summary>
    /// <param name="terminateDeduction">The deduction entity with termination information to update.</param>
    public async Task TerminateMedicalAidDeductionAsync(MedicalAidDeduction terminateDeduction)
    {
      _context.MedicalAidDeductions.Update(terminateDeduction);
      await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all inactive medical aid deduction records from a specific previous payroll run.
    /// </summary>
    /// <param name="previousRunNumber">The identifier of the previous payroll run.</param>
    /// <returns>A list of inactive medical aid deductions from the specified previous payroll run.</returns>
    public async Task<List<MedicalAidDeduction>> GetAllRecordsFromPreviousRun(int previousRunNumber)
    {
      return await _context.MedicalAidDeductions
        .Include(d => d.PayrollRun)
        .Where(d =>
          !d.IsActive &&
          d.PayrollRun != null && d.PayrollRunId == previousRunNumber &&
          d.PayrollRun.IsFinalised &&
          d.PayrollRun.IsLocked)
        .ToListAsync();
    }

    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
      await _context.SaveChangesAsync();
    }
  }
}