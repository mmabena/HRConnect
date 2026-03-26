namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Text.Json;
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;

  [DisallowConcurrentExecution]
  public class EmployeePensionEnrollmentJob(IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository,
    IEmployeeRepository employeeRepository, IPayrollRunRepository payrollRunRepository, IPensionDeductionRepository pensionDeductionRepository,
    ApplicationDBContext context) : IJob
  {
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IPensionDeductionRepository _pensionDeductionRepository = pensionDeductionRepository;
    private readonly ApplicationDBContext _context = context;
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;

    ///<summary>
    ///Scheduled quartz job to enroll employee to pension based on their pension option with their calculated pension deduction 
    ///</summary>
    public async Task Execute(IJobExecutionContext context)
    {
      string jsonFromscheudleJob = context.MergedJobDataMap.GetString("PensionEnrollment");
      EmployeePensionEnrollment? employeePensionEnrollment = JsonSerializer.Deserialize<EmployeePensionEnrollment>(jsonFromscheudleJob);
      if (employeePensionEnrollment != null)
      {
        Employee? employeeToPensionEnrollment = await _employeeRepository.GetEmployeeByIdAsync(employeePensionEnrollment.EmployeeId);
        if (employeeToPensionEnrollment != null)
        {
          if (!employeeToPensionEnrollment.PensionOptionId.HasValue)
          {
            throw new NotFoundException("Employee pension option was not found");
          }

          PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");

          EmployeePensionEnrollment employeePensionEnroll = new EmployeePensionEnrollment
          {
            EmployeeId = employeeToPensionEnrollment.EmployeeId,
            PensionOptionId = (int)employeeToPensionEnrollment.PensionOptionId,
            StartDate = employeeToPensionEnrollment.StartDate,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
            PayrollRunId = currentPayRollRun.PayrollRunId,
            VoluntaryContribution = (employeePensionEnrollment.VoluntaryContribution > decimal.Zero) ?
            employeePensionEnrollment.VoluntaryContribution : decimal.Zero,
            IsVoluntaryContributionPermament = (employeePensionEnrollment.VoluntaryContribution > decimal.Zero) ?
            employeePensionEnrollment.IsVoluntaryContributionPermament : null,
            IsLocked = false
          };

          EmployeePensionEnrollment? employeeExisitingPensionEnrollment = await _employeePensionEnrollmentRepository.
            GetByEmployeeIdAndLastRunIdAsync(employeeToPensionEnrollment.EmployeeId);

          if (employeeExisitingPensionEnrollment == null || (employeeExisitingPensionEnrollment.PayrollRunId != employeePensionEnroll.PayrollRunId))
          {
            _ = await _employeePensionEnrollmentRepository.AddAsync(employeePensionEnroll);
            await HandlePensionEnrollment(employeePensionEnroll);
          }
        }
        else
        {
          throw new NotFoundException("Employee was not found");
        }
      }
    }

    ///<summary>
    ///Add pension enrollment with its deduction
    ///</summary>
    ///<param name="employeePensionEnrollment">Employee Pension Enrollment</param>
    private async Task HandlePensionEnrollment(EmployeePensionEnrollment employeePensionEnrollment)
    {
      Employee existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeePensionEnrollment.EmployeeId)
        ?? throw new NotFoundException("Employee not found");
      if (existingEmployee != null)
      {
        decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync((int)existingEmployee.PensionOptionId);
        PayrollRun? currentPayrollRunId = await _payrollRunRepository.GetCurrentRunAsync();
        PensionDeduction? existingPensionDeduction = await _pensionDeductionRepository
          .GetByEmployeeIdAndIsNotLockedAsync(employeePensionEnrollment.EmployeeId);
        if (existingPensionDeduction == null)
        {
          PensionDeduction employeesPensionDeduction = new()
          {
            EmployeeId = existingEmployee.EmployeeId,
            FirstName = existingEmployee.Name,
            LastName = existingEmployee.Surname,
            DateJoinedCompany = existingEmployee.StartDate,
            IdNumber = existingEmployee.IdNumber,
            Passport = existingEmployee.PassportNumber,
            TaxNumber = existingEmployee.TaxNumber,
            PensionableSalary = existingEmployee.MonthlySalary,
            PensionOptionId = (int)existingEmployee.PensionOptionId,
            PendsionCategoryPercentage = pensionOptionPercentage,
            PensionContribution = ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100))),
            VoluntaryContribution = employeePensionEnrollment.VoluntaryContribution,
            TotalPensionContribution =
              ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100)) +
              employeePensionEnrollment.VoluntaryContribution),
            EmailAddress = existingEmployee.Email,
            PhysicalAddress = existingEmployee.PhysicalAddress,
            PayrollRunId = currentPayrollRunId.PayrollRunId,
            CreatedDate = employeePensionEnrollment.EffectiveDate,
            IsActive = true
          };

          PensionDeduction? existingPensionDeductionForEmployee = await _pensionDeductionRepository
            .GetByEmployeeIdAndIsNotLockedAsync(employeePensionEnrollment.EmployeeId);

          if (existingPensionDeductionForEmployee == null || (existingPensionDeductionForEmployee.PayrollRunId != employeesPensionDeduction.PayrollRunId))
          {
            _ = await _pensionDeductionRepository.AddAsync(employeesPensionDeduction);
          }
        }
      }
    }

    ///<summary>
    ///Auxiliary method to get employee pension option percentage
    ///</summary>
    ///<param name="pensionOptionId">Pension Option Id</param>
    ///<returns>
    ///Pension option percentage
    ///</returns
    private async Task<decimal> GetEmployeePensionOptionPercentageAsync(int pensionOptionId)
    {
      decimal? employeePensionOption = await _context.PensionOptions.Where(po => po.PensionOptionId == pensionOptionId)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
      return employeePensionOption ?? throw new NotFoundException("Pension option not found");
    }

    ///<summary>
    ///Auxiliary method to validate pension contribution amount
    ///</summary>
    ///<param name="pensionDeductionAddDto">Employee's monthly pension contribution</param>
    ///<returns>
    ///Pension contribution amount that is not exceeding the maximum monthly contribution limit
    ///</returns
    private static decimal ValidPensionContribution(decimal pensionContribution)
    {
      return (pensionContribution > MAX_MONTHLYCONTRIBUTION) ? MAX_MONTHLYCONTRIBUTION : pensionContribution;
    }
  }
}
