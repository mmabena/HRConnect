namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.PayrollDeduction;
  using Microsoft.EntityFrameworkCore;

  public class EmployeeDeductionRepository(ApplicationDBContext context) : IEmployeeDeductionRepository
  {
    private readonly ApplicationDBContext _context = context;

    ///<summary>
    ///Adds a new EmployeeDeduction to the database.
    ///</summary>
    ///<param name="employeeDeduction">The Employee Deduction mdoel to be added.</param>
    ///<returns>
    ///The added EmployeeDeduction entity.
    ///</returns>
    public async Task<EmployeeDeduction> AddAsync(EmployeeDeduction employeeDeduction)
    {
      _ = await _context.EmployeeDeductions.AddAsync(employeeDeduction);
      _ = await _context.SaveChangesAsync();
      return employeeDeduction;
    }

    public async Task AddRangeAsync(List<EmployeeDeduction> employeeDeductions)
    {
      await _context.EmployeeDeductions.AddRangeAsync(employeeDeductions);
      _ = await _context.SaveChangesAsync();
    }

    ///<summary>
    ///Check if an EmployeeDeduction already exists for a given employee, deduction, and payroll run. 
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<param name="deductionId">Unique code for the deduction</param>
    ///<param name="payrollRunId">Pay roll run Id</param>
    ///<returns>
    ///The Employee Deduction entity if it exists
    ///</returns>
    public async Task<EmployeeDeduction?> CheckIfEmployeeDeductionExistsForCurrentPayrun(string employeeId, string deductionId, int payrollRunId)
    {
      return await _context.EmployeeDeductions.FirstOrDefaultAsync(ed => ed.EmployeeId == employeeId
        && ed.DeductionId == deductionId
        && ed.PayrollRunId == payrollRunId) ?? null;
    }

    ///<summary>
    ///Retrieves all EmployeeDeduction records from the database. 
    ///</summary>
    ///<returns>
    ///A list of EmployeeDeduction entities.
    ///</returns>
    public async Task<List<EmployeeDeduction>> GetAllAsync()
    {
      return await _context.EmployeeDeductions.ToListAsync();
    }

    ///<summary>
    ///Retrieves a list of EmployeeDeduction records associated with a specific DeductionId.
    ///</summary>
    ///<param name="deductionId">Unique code for the deduction</param>
    ///<returns>
    ///A list of EmployeeDeduction with matching deduction code/Id
    ///</returns>
    public async Task<List<EmployeeDeduction>> GetByDeductionIdAsync(string deductionId)
    {
      return await _context.EmployeeDeductions.Where(ed => ed.DeductionId == deductionId).ToListAsync();
    }

    ///<summary>
    ///Retreive all employee's deductions that are not locked 
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee's deductions that are not locked
    ///</returns>
    public async Task<List<EmployeeDeduction>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      return await _context.EmployeeDeductions.Where(ed => ed.EmployeeId == employeeId && !ed.IsLocked).ToListAsync();
    }

    ///<summary>
    ///Retreive all employee's deductions from last pay run 
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee's deductions from last pay run
    ///</returns>
    public async Task<List<EmployeeDeduction>> GetByEmployeeIdAndLastRunIdAsync(string employeeId)
    {
      EmployeeDeduction? lastEmployeeDeductions = await _context.EmployeeDeductions
        .Where(ed => ed.EmployeeId == employeeId)
        .OrderByDescending(ed => ed.PayrollRunId)
        .FirstOrDefaultAsync();

      if (lastEmployeeDeductions == null)
      {
        return [];
      }

      int previousPayrollRunId = lastEmployeeDeductions.PayrollRunId;

      List<EmployeeDeduction> lastEmployeeDeductionsList = await _context.EmployeeDeductions
        .Where(ed => ed.EmployeeId == employeeId && ed.PayrollRunId == previousPayrollRunId).ToListAsync();

      return lastEmployeeDeductionsList;
    }

    ///<summary>
    ///Retreive all employee's deductions 
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of all employee's deductions
    ///</returns>
    public async Task<List<EmployeeDeduction>> GetByEmployeeIdAsync(string employeeId)
    {
      return await _context.EmployeeDeductions.Where(ed => ed.EmployeeId == employeeId).ToListAsync();
    }

    ///<summary>
    ///Retreive all deductions for specified payroll run id 
    ///</summary>
    ///<param name="payrollRunId">Pay roll run Id</param>
    ///<returns>
    ///A list of deductions for current payroll run id 
    ///</returns>
    public async Task<List<EmployeeDeduction>> GetByPayrollRunIdAsync(int payrollRunId)
    {
      return await _context.EmployeeDeductions.Where(ed => ed.PayrollRunId == payrollRunId).ToListAsync();
    }

    ///<summary>
    ///Lock employee deductions
    ///</summary>
    ///<param name="employeeDeductions">List of employee deductions</param>
    public async Task LockEmployeeDeductionsAsync(List<EmployeeDeduction> employeeDeductions)
    {
      _context.EmployeeDeductions.UpdateRange(employeeDeductions);
      _ = await _context.SaveChangesAsync();
    }

    ///<summary>
    ///Update employee deduction 
    ///</summary>
    ///<param name="employeeDeduction">Employee deduction model to be updated</param>
    ///<returns>
    ///Updated employee deduction
    ///</returns>
    public async Task<EmployeeDeduction> UpdateAsync(EmployeeDeduction employeeDeduction)
    {
      _ = _context.EmployeeDeductions.Update(employeeDeduction);
      _ = await _context.SaveChangesAsync();
      return employeeDeduction;
    }

    public async Task UpdateRangeAsync(List<EmployeeDeduction> employeeDeductions)
    {
      _context.EmployeeDeductions.UpdateRange(employeeDeductions);
      _ = await _context.SaveChangesAsync();
    }
  }
}
