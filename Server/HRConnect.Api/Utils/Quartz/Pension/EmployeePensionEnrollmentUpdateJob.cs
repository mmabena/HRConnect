namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Text.Json;
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Services;

  public class EmployeePensionEnrollmentUpdateJob(IEmployeeRepository employeeRepository,
    IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository) : IJob
  {
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
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
            await HandlePensionOptionChange(employeeUpdatedPensionEnrollment.EmployeeId, employeeUpdatedPensionEnrollment.PensionOptionId);
          }
        }
      }
    }

    private async Task HandlePensionOptionChange(string employeeId, int newPensionOptionId)
    {
      Employee employeeNeedingAnUpdate = _employeeRepository.GetEmployeeByIdAsync(employeeId).Result ?? throw new NotFoundException("Employee not found");
      employeeNeedingAnUpdate.PensionOptionId = newPensionOptionId;
      Employee? updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employeeNeedingAnUpdate);
      if (updatedEmployee != null && updatedEmployee.PensionOptionId != newPensionOptionId)
      {
        throw new InvalidOperationException("Failed to update employee's pension option");
      }
    }
  }
}
