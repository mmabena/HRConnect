namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Pension;

  public interface IEmployeePensionEnrollmentRepository
  {
    Task<EmployeePensionEnrollment> AddAsync(EmployeePensionEnrollment employeePensionEnrollment);
    Task<List<EmployeePensionEnrollment>> GetAllAsync();
    Task<EmployeePensionEnrollment?> GetByEmployeeIdAndLastRunIdAsync(string employeeId);
    Task<List<EmployeePensionEnrollment>> GetLatestEnrollmentForEachEmployeeAsync();
    Task<EmployeePensionEnrollment?> GetByEmployeeIdAndIsNotLockedAsync(string employeeId);
    Task<EmployeePensionEnrollment?> GetByEmployeeIdAsync(string employeeId);
    Task<List<EmployeePensionEnrollment>> GetByPayRollRunIdAsync(int payrollRunId);
    Task<List<EmployeePensionEnrollment>> GetEmployeePensionEnrollmentsNotLocked();
    Task<EmployeePensionEnrollment> UpdateAsync(EmployeePensionEnrollment employeePensionEnrollment);
    Task LockEmployeePensionEnrollmentsAsync(List<EmployeePensionEnrollment> employeePensionEnrollments);
  }
}
