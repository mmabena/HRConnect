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

    public async Task<EmployeePensionEnrollment> AddAsync(EmployeePensionEnrollment employeePensionEnrollment)
    {
      _ = await _context.EmployeePensionEnrollments.AddAsync(employeePensionEnrollment);
      _ = await _context.SaveChangesAsync();
      return employeePensionEnrollment;
    }

    public async Task<bool> DeleteAsync(EmployeePensionEnrollment employeePensionEnrollment)
    {
      EmployeePensionEnrollment? existingPensionEnrollmentEntity = await _context.EmployeePensionEnrollments.
        FirstOrDefaultAsync(epe => epe.EmployeeId == employeePensionEnrollment.EmployeeId);

      if (existingPensionEnrollmentEntity == null)
      {
        return false;
      }

      _ = _context.EmployeePensionEnrollments.Remove(employeePensionEnrollment);
      _ = await _context.SaveChangesAsync();
      return true;
    }

    public async Task<List<EmployeePensionEnrollment>> GetAllAsync()
    {
      return await _context.EmployeePensionEnrollments.ToListAsync();
    }

    public async Task<EmployeePensionEnrollment?> GetByEmployeeIdAsync(string employeeId)
    {
      EmployeePensionEnrollment? existEmployeesPensionEnrollment = await _context.EmployeePensionEnrollments
        .FirstOrDefaultAsync(epe => epe.EmployeeId == employeeId);

      return existEmployeesPensionEnrollment ?? null;
    }

    public async Task<List<EmployeePensionEnrollment>> GetByPayRollRunIdAsync(int payrollRunId)
    {
      return await _context.EmployeePensionEnrollments.Where(epe => epe.PayrollRunId == payrollRunId).ToListAsync();
    }

    public async Task<EmployeePensionEnrollment> UpdateAsync(EmployeePensionEnrollment employeePensionEnrollment)
    {
      _ = _context.EmployeePensionEnrollments.Update(employeePensionEnrollment);
      _ = await _context.SaveChangesAsync();
      return employeePensionEnrollment;
    }
  }
}
