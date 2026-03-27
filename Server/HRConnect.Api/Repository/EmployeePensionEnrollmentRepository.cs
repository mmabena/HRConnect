namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Pension;
  using Microsoft.EntityFrameworkCore;

  public class EmployeePensionEnrollmentRepository(ApplicationDBContext context) : IEmployeePensionEnrollmentRepository
  {
    private readonly ApplicationDBContext _context = context;

    ///<summary>
    ///Save employee pension enrollment to the database
    ///</summary>
    ///<param name="employeePensionEnrollment">Employee Pension Enrollment Model</param>
    ///<returns>
    ///Employees pension enrollment
    ///</returns>
    public async Task<EmployeePensionEnrollment> AddAsync(EmployeePensionEnrollment employeePensionEnrollment)
    {
      _ = await _context.EmployeePensionEnrollments.AddAsync(employeePensionEnrollment);
      _ = await _context.SaveChangesAsync();
      return employeePensionEnrollment;
    }

    ///<summary>
    ///Get all pension enrollments from the database
    ///</summary>
    ///<returns>
    ///All pension enrollments
    ///</returns>
    public async Task<List<EmployeePensionEnrollment>> GetAllAsync()
    {
      return await _context.EmployeePensionEnrollments.ToListAsync();
    }

    ///<summary>
    ///Get employee pension enrollment by employee id and is not locked from the database
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Employees pension enrollment with the given employee id and is not locked, otherwise null
    ///</returns>
    public Task<EmployeePensionEnrollment?> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      return _context.EmployeePensionEnrollments
        .FirstOrDefaultAsync(epe => epe.EmployeeId == employeeId && !epe.IsLocked);
    }

    ///<summary>
    ///Add employee pension enrollment to the database
    ///</summary>
    ///<param name="employeePensionEnrollment">Employee Pension Enrollment Model</param>
    ///<returns>
    ///Employees pension enrollment
    ///</returns>
    public async Task<EmployeePensionEnrollment?> GetByEmployeeIdAndLastRunIdAsync(string employeeId)
    {
      EmployeePensionEnrollment? employeeLatestPensionEnrollment = await _context.EmployeePensionEnrollments
        .Where(epe => epe.EmployeeId == employeeId)
        .OrderByDescending(epe => epe.EffectiveDate)
        .FirstOrDefaultAsync();

      return employeeLatestPensionEnrollment ?? null;
    }

    ///<summary>
    ///Get employee pension enrollment by payroll run id from the database
    ///</summary>
    ///<param name="payrollRunId">Pay roll run Id</param>
    ///<returns>
    ///List of employee pension enrollments with the given payroll run id, otherwise empty list
    ///</returns>
    public async Task<List<EmployeePensionEnrollment>> GetByPayRollRunIdAsync(int payrollRunId)
    {
      return await _context.EmployeePensionEnrollments.Where(epe => epe.PayrollRunId == payrollRunId).ToListAsync();
    }

    ///<summary>
    ///Get all employee pension enrollments that are not locked from the database
    ///</summary>
    ///<returns>
    ///List of employee pension enrollments that are not locked, otherwise empty list
    ///</returns>
    public async Task<List<EmployeePensionEnrollment>> GetEmployeePensionEnrollmentsNotLocked()
    {
      return await _context.EmployeePensionEnrollments.Where(epe => !epe.IsLocked).ToListAsync();
    }

    ///<summary>
    ///Lock all employee pension enrollments for the current pay roll run in the database
    ///</summary>
    ///<param name="employeePensionEnrollments">List of employee pension plans</param>
    ///<returns>
    ///Employees pension enrollment
    ///</returns>
    public async Task LockEmployeePensionEnrollmentsAsync(List<EmployeePensionEnrollment> employeePensionEnrollments)
    {
      _context.EmployeePensionEnrollments.UpdateRange(employeePensionEnrollments);
      _ = await _context.SaveChangesAsync();
    }

    ///<summary>
    ///Update employee pension enrollment in the database
    ///</summary>
    ///<param name="employeePensionEnrollment">Employee Pension Enrollment Model</param>
    ///<returns>
    ///Employee's updated pension enrollment
    ///</returns>
    public async Task<EmployeePensionEnrollment> UpdateAsync(EmployeePensionEnrollment employeePensionEnrollment)
    {
      _ = _context.EmployeePensionEnrollments.Update(employeePensionEnrollment);
      _ = await _context.SaveChangesAsync();
      return employeePensionEnrollment;
    }
  }
}
