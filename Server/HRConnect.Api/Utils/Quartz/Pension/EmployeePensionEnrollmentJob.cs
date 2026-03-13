namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Services;

  public class EmployeePensionEnrollmentJob(IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository,
    IEmployeeRepository employeeRepository, IPayrollRunRepository payrollRunRepository) : IJob
  {
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task Execute(IJobExecutionContext context)
    {
      string? employeeId = context.JobDetail.JobDataMap.GetString("EmployeeId");
      if (!string.IsNullOrEmpty(employeeId))
      {
        Employee? employeeToPensionEnrollment = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (employeeToPensionEnrollment != null)
        {
          if (!employeeToPensionEnrollment.PensionOptionId.HasValue)
          {
            throw new NotFoundException("Employee pension option was not found");
          }

          PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
          EmployeePensionEnrollment employeePensionEnrollment = new EmployeePensionEnrollment
          {
            EmployeeId = employeeToPensionEnrollment.EmployeeId,
            PensionOptionId = employeeToPensionEnrollment.PensionOptionId.Value,
            StartDate = employeeToPensionEnrollment.StartDate,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
            PayrollRunId = currentPayRollRun.PayrollRunId,
            IsLocked = false
          };

          _ = await _employeePensionEnrollmentRepository.AddAsync(employeePensionEnrollment);
        }
        else
        {
          throw new NotFoundException("Employee was not found");
        }
      }
    }
  }
}
