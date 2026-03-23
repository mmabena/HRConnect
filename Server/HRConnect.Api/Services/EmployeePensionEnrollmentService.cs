namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.DTOs.Payroll.Pension;
  //using HRConnect.Api.DTOs.Payroll.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  //using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Utils.Pension.ValidationHelpers;
  using HRConnect.Api.Utils.Quartz.Pension;
  using Microsoft.EntityFrameworkCore;
  using Quartz;
  //using HRConnect.Api.Utils.Quartz;

  //using Microsoft.EntityFrameworkCore;

  public class EmployeePensionEnrollmentService(IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository,
    IEmployeeRepository employeeRepository, IPayrollRunRepository payrollRunRepository, IPensionDeductionRepository pensionDeductionRepository,
    ISchedulerFactory scheduler, ApplicationDBContext context) : IEmployeePensionEnrollmentService
  {
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly IPensionDeductionRepository _pensionDeductionRepository = pensionDeductionRepository;
    private readonly ISchedulerFactory _schedulerFactory = scheduler;
    private readonly ApplicationDBContext _context = context;
    //private static readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;

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
      if (existingEmployeePensionEnrollment != null && !existingEmployeePensionEnrollment.IsLocked)
      {
        throw new InvalidOperationException("Employee pension enrollment already exists for this employee");
      }

      employeePensionEnrollment.PensionOptionId = existingEmployee.PensionOptionId.Value;
      decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync((int)existingEmployee.PensionOptionId);
      ValidateEmployeePensionEnrollmentDtos.ValidateVoluntaryContribution((decimal)employeePensionEnrollmentDto.VoluntaryContribution, existingEmployee.MonthlySalary, pensionOptionPercentage);
      employeePensionEnrollment.StartDate = existingEmployee.StartDate;
      PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      employeePensionEnrollment.VoluntaryContribution = (decimal)employeePensionEnrollmentDto.VoluntaryContribution;
      employeePensionEnrollment.IsVoluntaryContributionPermament = (employeePensionEnrollmentDto.VoluntaryContribution > decimal.Zero) ?
        employeePensionEnrollmentDto.IsVoluntaryContributionPermament : null;
      employeePensionEnrollment.PayrollRunId = currentPayRollRun.PayrollRunId;
      employeePensionEnrollment.IsLocked = false;

      EmployeePensionEnrollment addedEmployeePensionEnrollment;
      DateOnly today = DateOnly.FromDateTime(DateTime.Today);
      if (today.Day > 15 || (employeePensionEnrollmentDto.EffectiveDate.Day > 15))
      {
        DateOnly firstDayNextMonth = new DateOnly(existingEmployee.StartDate.Year, existingEmployee.StartDate.Month, 1).AddMonths(1);
        employeePensionEnrollment.EffectiveDate = firstDayNextMonth;
        //Qaurtz reschedule
        string serializedEmployeePensionEnrollmentAddDto = JsonSerializer.Serialize(employeePensionEnrollment);
        IJobDetail job = JobBuilder.Create<EmployeePensionEnrollmentJob>()
          .WithIdentity($"pensionenrollmentjob_{existingEmployee.EmployeeId}")
          .UsingJobData("PensionEnrollment", serializedEmployeePensionEnrollmentAddDto)
          .Build();

        ITrigger trigger = TriggerBuilder.Create()
          //.StartAt(employeePensionEnrollment.EffectiveDate.ToDateTime(TimeOnly.MinValue))
          .StartAt(DateBuilder.FutureDate(15, IntervalUnit.Second))
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
        await HandlePensionEnrollment(addedEmployeePensionEnrollment);
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
      Employee? existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeePensionEnrollmentUpdateDto.EmployeeId);
      if (existingEmployee != null && employeePensionEnrollmentUpdateDto.VoluntaryContribution != null)
      {
        decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync((int)existingEmployee.PensionOptionId);
        ValidateEmployeePensionEnrollmentDtos.ValidateVoluntaryContribution(
        (decimal)employeePensionEnrollmentUpdateDto.VoluntaryContribution, existingEmployee.MonthlySalary, pensionOptionPercentage);
      }
      else
      {
        throw new NotFoundException("Employee does not exist");
      }

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

        DateTime today = DateTime.Now;
        if (today.Day >= 16)
        {
          DateOnly firstDayNextMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
          employeePensionEnrollment.EffectiveDate = firstDayNextMonth;
          //Qaurtz reschedule
          string serializedEmployeePensionEnrollmentUpdateDto = JsonSerializer.Serialize(employeePensionEnrollmentUpdateDto);
          IJobDetail job = JobBuilder.Create<EmployeePensionEnrollmentUpdateJob>()
            .WithIdentity($"pensionenrollmentUpdatejob_{existingEmployee.EmployeeId}")
            .UsingJobData("UpdatedEnrollment", serializedEmployeePensionEnrollmentUpdateDto)
            .Build();

          ITrigger trigger = TriggerBuilder.Create()
            //.StartAt(employeePensionEnrollment.EffectiveDate.ToDateTime(TimeOnly.MinValue))
            .StartAt(DateBuilder.FutureDate(40, IntervalUnit.Second))
            .Build();

          IScheduler schedulerInstance = await _schedulerFactory.GetScheduler();
          _ = await schedulerInstance.ScheduleJob(job, trigger);

          EmployeePensionEnrollmentDto responseEmployeePensionEnrollmentDto = employeePensionEnrollment.ToEmployeePensionEnrollmentDto();
          responseEmployeePensionEnrollmentDto.WarningMessage =
            "Employee pension enrollment update has been scheduled for the next month as the payday date has already occured.";

          return responseEmployeePensionEnrollmentDto;
        }
        else
        {
          EmployeePensionEnrollment employeeUpdatedPensionEnrollment = await _employeePensionEnrollmentRepository
          .UpdateAsync(employeePensionEnrollment);
          if (employeePensionEnrollmentUpdateDto.PensionOptionId.HasValue &&
            employeeUpdatedPensionEnrollment.PensionOptionId != oldPensionOptionId)
          {
            await HandlePensionOptionChange(employeeUpdatedPensionEnrollment.EmployeeId, 
              employeeUpdatedPensionEnrollment.PensionOptionId, employeePensionEnrollmentUpdateDto.VoluntaryContribution);
          }

          return employeeUpdatedPensionEnrollment.ToEmployeePensionEnrollmentDto();
        }
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

    private async Task<decimal> GetEmployeePensionOptionPercentageAsync(int pensionOptionId)
    {
      decimal? employeePensionOption = await _context.PensionOptions.Where(po => po.PensionOptionId == pensionOptionId)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
      return employeePensionOption ?? throw new NotFoundException("Pension option not found");
    }

    private async Task HandlePensionEnrollment(EmployeePensionEnrollment employeePensionEnrollment)
    {
      Employee existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeePensionEnrollment.EmployeeId)
        ?? throw new NotFoundException("Employee not found");
      if (existingEmployee != null)
      {
        decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync((int)existingEmployee.PensionOptionId);
        PayrollRun? currentPayrollRunId = await _payrollRunRepository.GetCurrentRunAsync();
        PensionDeduction? existingPensionDeduction = await _pensionDeductionRepository.GetByEmployeeIdAndIsNotLockedAsync(employeePensionEnrollment.EmployeeId);
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
            PensionContribution = ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100))),
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

    public async Task<List<EmployeePensionEnrollmentDto>> GetPensionEnrollementsNotLocked()
    {
      List<EmployeePensionEnrollment> pensionEnrollments = await _employeePensionEnrollmentRepository.GetEmployeePensionEnrollmentsNotLocked();
      return pensionEnrollments.Select(epe => epe.ToEmployeePensionEnrollmentDto()).ToList();
    }

    private static decimal ValidPensionContribution(decimal pensionContribution)
    {
      return (pensionContribution > MAX_MONTHLYCONTRIBUTION) ? MAX_MONTHLYCONTRIBUTION : pensionContribution;
    }
  }
}
