namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Services;

  public class EmployeePensionRollOverJob(IPayrollRunRepository payrollRunRepository,
    IEmployeeRepository employeeRepository, IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository) : IJob
  {
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    public async Task Execute(IJobExecutionContext context)
    {
      PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> employeesWithPensionOption = await _employeeRepository.GetAllEmployeeWithAPensionOption();

      foreach (Employee employee in employeesWithPensionOption)
      {
        EmployeePensionEnrollment employeePensionEnrollment = new()
        {
          EmployeeId = employee.EmployeeId,
          PayrollRunId = currentPayRollRun.PayrollRunId,
          PensionOptionId = employee.PensionOptionId.Value,
          EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
          IsLocked = false,
        };

        _ = await _employeePensionEnrollmentRepository.AddAsync(employeePensionEnrollment);
      }
    }
  }
}
