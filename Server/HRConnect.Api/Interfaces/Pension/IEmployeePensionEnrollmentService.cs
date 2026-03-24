namespace HRConnect.Api.Interfaces.Pension
{
  using HRConnect.Api.DTOs.Employee.Pension;

  public interface IEmployeePensionEnrollmentService
  {
    Task<EmployeePensionEnrollmentDto> AddEmployeePensionEnrollmentAsync(EmployeePensionEnrollmentAddDto employeePensionEnrollmentDto);
    Task<List<EmployeePensionEnrollmentDto>> GetAllEmployeePensionEnrollementsAsync();
    Task<EmployeePensionEnrollmentDto?> GetEmployeePensionEnrollementByIdAsync(string employeeId);
    Task<List<EmployeePensionEnrollmentDto>> GetPensionEnrollementsByPayRollRunIdAsync(int payrollRunId);
    Task<List<EmployeePensionEnrollmentDto>> GetPensionEnrollementsNotLocked();
    Task<EmployeePensionEnrollmentDto> UpdateEmployeePensionEnrollementAsync(EmployeePensionEnrollmentUpdateDto employeePensionEnrollmentUpdateDto);
    Task<bool> DeleteEmployeePensionEnrollementAsync();
    Task LockEmployeePensionEnrollmentsAsync();
    Task RollOverEmloyeePensionEnrollmentAsync();
  }
}
