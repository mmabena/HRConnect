namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.PayrollDeduction;
  using Microsoft.EntityFrameworkCore;

  public class PensionDeductionRepository(ApplicationDBContext context) : IPensionDeductionRepository
  {
    private readonly ApplicationDBContext _context = context;

    ///<summary>
    ///Save employee pension deduction to the database
    ///</summary>
    ///<param name="pensionDeduction">Employee Pension Deduction Model</param>
    ///<returns>
    ///Employees pension deduction model that was added to the database
    ///</returns>
    public async Task<PensionDeduction> AddAsync(PensionDeduction pensionDeduction)
    {
      _ = await _context.PensionDeductions.AddAsync(pensionDeduction);
      _ = await _context.SaveChangesAsync();
      return pensionDeduction;
    }

    ///<summary>
    ///Get all pension deductions from the database
    ///</summary>
    ///<returns>
    ///All pension deductions from the database
    ///</returns>
    public async Task<List<PensionDeduction>> GetAllAsync()
    {
      return await _context.PensionDeductions.ToListAsync();
    }

    ///<summary>
    ///Get employee pension deduction by employee id and is not locked from the database
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Employees pension deduction and is not locked from the database
    ///</returns>
    public async Task<PensionDeduction?> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      PensionDeduction? existingPensionDeduction = await _context.PensionDeductions
        .FirstOrDefaultAsync(pd => pd.EmployeeId == employeeId && !pd.IsLocked);

      return existingPensionDeduction;
    }

    ///<summary>
    ///Get employee pension deduction with matching employee Id and last payroll run id and is not locked from the database
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<param name="payRollRunId">Pay roll run Id</param>
    ///<returns>
    ///Employees pension deduction with matching employeeId and payroll run id and is not locked from the database
    ///</returns>
    public async Task<PensionDeduction?> GetByEmployeeIdAndLastRunIdAsync(string employeeId, int payRollRunId)
    {
      PensionDeduction? existingPensionDeduction = await _context.PensionDeductions
        .FirstOrDefaultAsync(pd => pd.EmployeeId == employeeId && pd.PayrollRunId == payRollRunId && !pd.IsLocked);

      return existingPensionDeduction;
    }

    ///<summary>
    ///Get employee latest pension deduction by employee id from the database
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Employee's latest pension deduction with matching employee id from the database
    ///</returns>
    public async Task<PensionDeduction?> GetByEmployeeIdAsync(string employeeId)
    {
      PensionDeduction? existingPensionDeduction = await _context.PensionDeductions
        .Where(pd => pd.EmployeeId == employeeId)
        .OrderByDescending(pd => pd.CreatedDate)
        .FirstOrDefaultAsync();

      return existingPensionDeduction ?? null;
    }

    ///<summary>
    ///Get all pension deductions with matching last payroll run id from the database
    ///</summary>
    ///<param name="payRollRunId">Pay roll run Id</param>
    ///<returns>
    ///Pension deductions with matching payroll run id from the database
    ///</returns>
    public async Task<List<PensionDeduction>> GetByPayRollRunIdAsync(int payrollRunId)
    {
      return await _context.PensionDeductions.Where(pd => pd.PayrollRunId == payrollRunId).ToListAsync();
    }

    ///<summary>
    ///Update employee pension deduction details in the database
    ///</summary>
    ///<param name="pensionDeduction">Pension Deduction Model</param>
    ///<returns>
    ///Employees updated pension deduction details
    ///</returns>
    public async Task<PensionDeduction> UpdateAsync(PensionDeduction pensionDeduction)
    {
      _ = _context.PensionDeductions.Update(pensionDeduction);
      _ = await _context.SaveChangesAsync();
      return pensionDeduction;
    }
  }
}
