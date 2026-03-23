namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Text.Json;
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Repository;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;

  [DisallowConcurrentExecution]
  public class EmployeePensionEnrollmentUpdateJob(IEmployeeRepository employeeRepository,
    IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository, IPensionDeductionRepository pensionDeductionRepository,
    ApplicationDBContext context) : IJob
  {
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IPensionDeductionRepository _pensionDeductionRepository = pensionDeductionRepository;
    private readonly ApplicationDBContext _context = context;
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;
    public async Task Execute(IJobExecutionContext context)
    {
      string jsonFromScheduleJob = context.MergedJobDataMap.GetString("UpdatedEnrollment");
      EmployeePensionEnrollmentUpdateDto? employeePensionEnrollmentUpdateDto =
        JsonSerializer.Deserialize<EmployeePensionEnrollmentUpdateDto>(jsonFromScheduleJob);

      if (employeePensionEnrollmentUpdateDto != null)
      {
        EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository.
        GetByEmployeeIdAsync(employeePensionEnrollmentUpdateDto.EmployeeId);

        int oldPensionOptionId = (int)(employeePensionEnrollment?.PensionOptionId);
        if (employeePensionEnrollment != null)
        {
          employeePensionEnrollment.PensionOptionId = employeePensionEnrollmentUpdateDto.PensionOptionId
          ?? employeePensionEnrollment.PensionOptionId;
          employeePensionEnrollment.EffectiveDate = employeePensionEnrollmentUpdateDto.EffectiveDate
            ?? employeePensionEnrollment.EffectiveDate;
          employeePensionEnrollment.VoluntaryContribution = employeePensionEnrollmentUpdateDto.VoluntaryContribution
            ?? employeePensionEnrollment.VoluntaryContribution;
          employeePensionEnrollment.IsVoluntaryContributionPermament = employeePensionEnrollmentUpdateDto.IsVoluntaryContributionPermament
            ?? employeePensionEnrollment.IsVoluntaryContributionPermament;

          EmployeePensionEnrollment employeeUpdatedPensionEnrollment = await _employeePensionEnrollmentRepository
          .UpdateAsync(employeePensionEnrollment);

          if (employeePensionEnrollmentUpdateDto.PensionOptionId.HasValue &&
            employeeUpdatedPensionEnrollment.PensionOptionId != oldPensionOptionId)
          {
            await HandlePensionOptionChange(
              employeeUpdatedPensionEnrollment.EmployeeId, employeeUpdatedPensionEnrollment.PensionOptionId,
              employeeUpdatedPensionEnrollment.VoluntaryContribution);
          }
        }
      }
    }

    private async Task HandlePensionOptionChange(string employeeId, int newPensionOptionId, decimal? voluntaryContribution)
    {
      Employee employeeNeedingAnUpdate = _employeeRepository.GetEmployeeByIdAsync(employeeId).Result ?? throw new NotFoundException("Employee not found");
      employeeNeedingAnUpdate.PensionOptionId = newPensionOptionId;
      Employee? updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employeeNeedingAnUpdate);
      if (updatedEmployee != null && updatedEmployee.PensionOptionId != newPensionOptionId)
      {
        throw new InvalidOperationException("Failed to update employee's pension option");
      }

      PensionDeduction pensionDeduction = _pensionDeductionRepository.GetByEmployeeIdAndIsNotLockedAsync(employeeId).Result
        ?? throw new NotFoundException("Pension deduction for employee not found");
      pensionDeduction.PensionOptionId = newPensionOptionId;
      decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync(newPensionOptionId);
      pensionDeduction.PendsionCategoryPercentage = pensionOptionPercentage;
      pensionDeduction.VoluntaryContribution = voluntaryContribution ?? pensionDeduction.VoluntaryContribution;
      pensionDeduction.PensionContribution = ValidPensionContribution(
        Math.Round(pensionDeduction.PensionableSalary * (pensionOptionPercentage / 100), 2));
      PensionDeduction? updatedPensionDeduction = await _pensionDeductionRepository.UpdateAsync(pensionDeduction);
      if (updatedPensionDeduction != null && updatedPensionDeduction.PensionOptionId != newPensionOptionId)
      {
        throw new InvalidOperationException("Failed to update pension deduction with new pension option");
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
