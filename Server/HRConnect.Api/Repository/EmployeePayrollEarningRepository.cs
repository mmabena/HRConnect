namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll.Earnings;
  using Microsoft.EntityFrameworkCore;

  public class EmployeePayrollEarningRepository(ApplicationDBContext context) : IEmployeePayrollEarningRepository
  {
    private readonly ApplicationDBContext _context = context;

    ///<summary>
    ///Add new employee payroll earning to the database
    ///</summary>
    ///<param name="employeePayrollEarning">Employee pay roll earning model</param>
    ///<returns></returns>
    public async Task<EmployeePayrollEarning> AddAsync(EmployeePayrollEarning employeePayrollEarning)
    {
      _ = await _context.EmployeePayrollEarnings.AddAsync(employeePayrollEarning);
      _ = await _context.SaveChangesAsync();
      return employeePayrollEarning;
    }

    public async Task<EmployeePayrollEarning?> CheckIfEmployeeEarningExistsForCurrentPayrun(string employeeId, string payrollEarningId, int payrollRunId)
    {
      return await _context.EmployeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId
        && epe.PayrollEarningId == payrollEarningId
        && epe.PayrollRunId == payrollRunId)
        .FirstOrDefaultAsync() ?? null;
    }

    ///<summary>
    ///Retrieve all employee payroll earnings from the database
    ///</summary>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing all employee payroll earnings in the database
    ///</returns>
    public async Task<List<EmployeePayrollEarning>> GetAllAsync()
    {
      return await _context.EmployeePayrollEarnings.ToListAsync();
    }

    ///<summary>
    ///Retrieve employee payroll earnings that are not locked by employee Id from the database
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing employee payroll earnings that are not locked with the given employee Id in the database
    ///</returns>
    public async Task<List<EmployeePayrollEarning>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      return await _context.EmployeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId && !epe.IsLocked).ToListAsync();
    }

    /// <summary>
    ///Retrieve employee payroll earnings by employee Id and last payroll run Id from the database. 
    /// </summary>
    /// <param name="employeeId"></param>
    /// <returns></returns>
    public async Task<List<EmployeePayrollEarning>> GetByEmployeeIdAndLastRunIdAsync(string employeeId)
    {
      EmployeePayrollEarning? lastEmployeePayrollEarnings = await _context.EmployeePayrollEarnings
        .Where(epe => epe.EmployeeId == employeeId)
        .OrderByDescending(epe => epe.PayrollRunId)
        .FirstOrDefaultAsync();

      if (lastEmployeePayrollEarnings == null)
      {
        return [];
      }

      int previousPayrollRunId = lastEmployeePayrollEarnings.PayrollRunId;

      List<EmployeePayrollEarning> lastEmployeePayrollEarningsList = await _context.EmployeePayrollEarnings
        .Where(epe => epe.EmployeeId == employeeId && epe.PayrollRunId == previousPayrollRunId)
        .ToListAsync();

      return lastEmployeePayrollEarningsList;
    }

    ///<summary>
    ///Retrieve employee payroll earnings by employee Id from the database  
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing employee payroll earnings with the given employee Id in the database
    ///</returns>
    public async Task<List<EmployeePayrollEarning>> GetByEmployeeIdAsync(string employeeId)
    {
      return await _context.EmployeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId).ToListAsync();
    }

    ///<summary>
    ///Retrieve employee payroll earnings by payroll earning Id from the database
    ///</summary>
    ///<param name="payrollEarningId">Pay roll earning Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing employee payroll earnings with the given payroll earning Id in the database
    ///</returns>
    public async Task<List<EmployeePayrollEarning>> GetByPayrollEarningIdAsync(string payrollEarningId)
    {
      return await _context.EmployeePayrollEarnings.Where(epe => epe.PayrollEarningId == payrollEarningId).ToListAsync();
    }

    ///<summary>
    ///Retrieve employee payroll earnings by payroll run Id from the database
    ///</summary>
    ///<param name="payrollRunId">Pay roll run Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing employee payroll earnings with the given payroll run Id in the database
    ///</returns>
    public Task<List<EmployeePayrollEarning>> GetByPayrollRunIdAsync(int payrollRunId)
    {
      return _context.EmployeePayrollEarnings.Where(epe => epe.PayrollRunId == payrollRunId).ToListAsync();
    }

    ///<summary>
    ///Retrieve employee payroll earnings by tax code from the database
    ///</summary>
    ///<param name="taxCode">Tax code</param>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing employee payroll earnings with the given tax code in the database
    ///</returns>
    public Task<List<EmployeePayrollEarning>> GetByTaxCodeAsync(int taxCode)
    {
      return _context.EmployeePayrollEarnings.Where(epe => epe.TaxCode == taxCode).ToListAsync();
    }

    ///<summary>
    ///Retrieve employee payroll earnings that are not locked by employee Id from the database
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarning objects representing employee payroll earnings that are not locked with the given employee Id in the database
    ///</returns>
    public Task<List<EmployeePayrollEarning>> GetEmployeePayrollEarningsNotLocked(string employeeId)
    {
      return _context.EmployeePayrollEarnings.Where(epe => epe.EmployeeId == employeeId && !epe.IsLocked).ToListAsync();
    }

    ///<summary>
    ///Lock employee payroll earnings in the database
    ///</summary>
    ///<param name="employeePayrollEarnings">A list of EmployeePayrollEarning objects to be locked</param>
    ///<returns>A task representing the asynchronous operation</returns>
    public async Task LockEmployeePayrollEarningsAsync(List<EmployeePayrollEarning> employeePayrollEarnings)
    {
      _context.EmployeePayrollEarnings.UpdateRange(employeePayrollEarnings);
      _ = await _context.SaveChangesAsync();
    }

    public async Task<EmployeePayrollEarning> UpdateAsync(EmployeePayrollEarning employeePayrollEarning)
    {
      _ = _context.EmployeePayrollEarnings.Update(employeePayrollEarning);
      _ = await _context.SaveChangesAsync();
      return employeePayrollEarning;
    }
  }
}
