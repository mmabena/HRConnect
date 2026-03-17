namespace HRConnect.Api.Utils.Quartz.Pension
{
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
          await HandlePensionEnrollment(employeePensionEnrollment);
        }
        else
        {
          throw new NotFoundException("Employee was not found");
        }
      }
    }

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
            IDNumber = existingEmployee.IdNumber,
            Passport = existingEmployee.PassportNumber,
            TaxNumber = existingEmployee.TaxNumber,
            PensionableSalary = existingEmployee.MonthlySalary,
            PensionOptionId = (int)existingEmployee.PensionOptionId,
            PendsionCategoryPercentage = pensionOptionPercentage,
            PensionContribution = ValidPensionContribution(existingEmployee.MonthlySalary * pensionOptionPercentage),
            VoluntaryContribution = employeePensionEnrollment.VoluntaryContribution,
            EmailAddress = existingEmployee.Email,
            PhyscialAddress = existingEmployee.PhysicalAddress,
            PayrollRunId = currentPayrollRunId.PayrollRunId,
            CreatedDate = employeePensionEnrollment.EffectiveDate,
            IsActive = true
          };


          _ = await _pensionDeductionRepository.AddAsync(employeesPensionDeduction);
        }
      }
    }
    private async Task<decimal> GetEmployeePensionOptionPercentageAsync(int pensionOptionId)
    {
      decimal? employeePensionOption = await _context.PensionOptions.Where(po => po.PensionOptionId == pensionOptionId)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
      return employeePensionOption ?? throw new NotFoundException("Pension option not found");
    }
    private static decimal ValidPensionContribution(decimal pensionContribution)
    {
      return (pensionContribution > MAX_MONTHLYCONTRIBUTION) ? MAX_MONTHLYCONTRIBUTION : pensionContribution;
    }
  }
}
