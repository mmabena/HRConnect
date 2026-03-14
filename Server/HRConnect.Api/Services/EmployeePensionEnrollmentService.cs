namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  //using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Utils.Pension.ValidationHelpers;
  using HRConnect.Api.Utils.Quartz.Pension;
  using Quartz;
  //using HRConnect.Api.Utils.Quartz;

  //using Microsoft.EntityFrameworkCore;

  public class EmployeePensionEnrollmentService(IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository,
    IEmployeeRepository employeeRepository, IPayrollRunRepository payrollRunRepository, ISchedulerFactory scheduler,
    ApplicationDBContext context) : IEmployeePensionEnrollmentService
  {
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly ISchedulerFactory _schedulerFactory = scheduler;
    private readonly ApplicationDBContext _context = context;

    public async Task<EmployeePensionEnrollmentDto> AddEmployeePensionEnrollmentAsync(EmployeePensionEnrollmentAddDto employeePensionEnrollmentDto)
    {
      ValidateAddEmployeesPensionEnrollment(employeePensionEnrollmentDto);
      EmployeePensionEnrollment employeePensionEnrollment = employeePensionEnrollmentDto.EmployeePensionEnrollmentToAddDTO();
      Employee? existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeePensionEnrollmentDto.EmployeeId)
        ?? throw new NotFoundException("Employee not found");

      if (!existingEmployee.PensionOptionId.HasValue)
      {
        throw new InvalidOperationException("Employee does not have a pension option assigned");
      }

      EmployeePensionEnrollment? existingEmployeePensionEnrollment = await _employeePensionEnrollmentRepository.GetByEmployeeIdAsync(employeePensionEnrollmentDto.EmployeeId);
      if (existingEmployeePensionEnrollment != null)
      {
        throw new InvalidOperationException("Employee pension enrollment already exists for this employee");
      }

      employeePensionEnrollment.PensionOptionId = existingEmployee.PensionOptionId.Value;
      employeePensionEnrollment.StartDate = existingEmployee.StartDate;
      PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      employeePensionEnrollment.PayrollRunId = currentPayRollRun.PayrollRunId;
      employeePensionEnrollment.IsLocked = false;

      EmployeePensionEnrollment addedEmployeePensionEnrollment;
      DateOnly today = DateOnly.FromDateTime(DateTime.Today);
      if (today.Day > 15 || (employeePensionEnrollmentDto.EffectiveDate.Day > 15))
      {
        DateOnly firstDayNextMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
        employeePensionEnrollment.EffectiveDate = firstDayNextMonth;
        //Qaurtz reschedule
        IJobDetail job = JobBuilder.Create<EmployeePensionEnrollmentJob>()
          .WithIdentity($"pensionenrollmentjob_{existingEmployee.EmployeeId}")
          .UsingJobData("EmployeeId", existingEmployee.EmployeeId)
          .Build();

        ITrigger trigger = TriggerBuilder.Create()
          //.StartAt(employeePensionEnrollment.EffectiveDate.ToDateTime(TimeOnly.MinValue))
          .StartAt(DateBuilder.FutureDate(40, IntervalUnit.Second))
          .Build();

        IScheduler schedulerInstance = await _schedulerFactory.GetScheduler();
        _ = await schedulerInstance.ScheduleJob(job, trigger);

        EmployeePensionEnrollmentDto responseEmployeePensionEnrollmentDto = employeePensionEnrollment.ToEmployeePensionEnrollmentDto();
        responseEmployeePensionEnrollmentDto.WarningMessage =
          "Employee pension enrollment has been scheduled for the next month as the effective date falls after the 15th of the month.";

        return responseEmployeePensionEnrollmentDto;
      }
      else
      {
        employeePensionEnrollment.EffectiveDate = employeePensionEnrollmentDto.EffectiveDate;
        addedEmployeePensionEnrollment = await _employeePensionEnrollmentRepository.AddAsync(employeePensionEnrollment);
        return addedEmployeePensionEnrollment.ToEmployeePensionEnrollmentDto();
      }
    }

    public Task<bool> DeleteEmployeePensionEnrollementAsync()
    {
      throw new NotImplementedException();
    }

    public async Task<List<EmployeePensionEnrollmentDto>> GetAllEmployeePensionEnrollementsAsync()
    {
      List<EmployeePensionEnrollment> pensionEnrollments = await _employeePensionEnrollmentRepository.GetAllAsync();
      return pensionEnrollments.Select(epe => epe.ToEmployeePensionEnrollmentDto()).ToList();
    }

    public async Task<EmployeePensionEnrollmentDto?> GetEmployeePensionEnrollementByIdAsync(string employeeId)
    {
      EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository.GetByEmployeeIdAsync(employeeId)
        ?? throw new NotFoundException("Employee not found");
      return employeePensionEnrollment.ToEmployeePensionEnrollmentDto();
    }

    public async Task<List<EmployeePensionEnrollmentDto>> GetPensionEnrollementsByPayRollRunIdAsync(int payrollRunId)
    {
      List<EmployeePensionEnrollment> pensionEnrollments = await _employeePensionEnrollmentRepository.GetByPayRollRunIdAsync(payrollRunId);
      return pensionEnrollments.Select(epe => epe.ToEmployeePensionEnrollmentDto()).ToList();
    }

    public async Task<EmployeePensionEnrollmentDto> UpdateEmployeePensionEnrollementAsync(EmployeePensionEnrollmentUpdateDto
      employeePensionEnrollmentUpdateDto)
    {
      ValidateEmployeePensionEnrollmentDtos.ValidateUpdateDto(employeePensionEnrollmentUpdateDto);
      EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository.
        GetByEmployeeIdAsync(employeePensionEnrollmentUpdateDto.EmployeeId);

      int oldPensionOptionId = employeePensionEnrollment?.PensionOptionId ?? 0;
      if (employeePensionEnrollment != null)
      {
        employeePensionEnrollment.PensionOptionId = employeePensionEnrollmentUpdateDto.PensionOptionId
          ?? employeePensionEnrollment.PensionOptionId;
        employeePensionEnrollment.EffectiveDate = employeePensionEnrollmentUpdateDto.EffectiveDate
          ?? employeePensionEnrollment.EffectiveDate;
        employeePensionEnrollment.PayrollRunId = employeePensionEnrollmentUpdateDto.PayrollRunId
          ?? employeePensionEnrollment.PayrollRunId;

        EmployeePensionEnrollment employeeUpdatedPensionEnrollment = await _employeePensionEnrollmentRepository
          .UpdateAsync(employeePensionEnrollment);
        if (employeePensionEnrollmentUpdateDto.PensionOptionId.HasValue &&
          employeeUpdatedPensionEnrollment.PensionOptionId != oldPensionOptionId)
        {
          await HandlePensionOptionChange(employeeUpdatedPensionEnrollment.EmployeeId, employeeUpdatedPensionEnrollment.PensionOptionId);
        }

        return employeeUpdatedPensionEnrollment.ToEmployeePensionEnrollmentDto();
      }
      else
      {
        throw new NotFoundException("Employee pension enrollment was not found");
      }
    }

    private static void ValidateAddEmployeesPensionEnrollment(EmployeePensionEnrollmentAddDto employeePensionEnrollmentDto)
    {
      //await CheckIfPensionOptionExists(employeePensionEnrollmentDto.PensionOptionId);
      ValidateEmployeePensionEnrollmentDtos.ValidateAddDto(employeePensionEnrollmentDto);
    }

    /*private async Task CheckIfPensionOptionExists(int pensionOptionId)
    {
      PensionOption? existingPensionOption = await _context.PensionOptions.FirstOrDefaultAsync(po => po.PensionOptionId == pensionOptionId) ??
        throw new NotFoundException("Pension option does not exist in the database");
    }*/

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
