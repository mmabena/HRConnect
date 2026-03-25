namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Utils.Pension.ValidationHelpers;
  using HRConnect.Api.Utils.Quartz.Pension;
  using Microsoft.EntityFrameworkCore;
  using Quartz;

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
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;

    ///<summary>
    ///Add employee pension enrollment
    ///</summary>
    ///<param name="employeePensionEnrollmentDto">Employee Pension Enrollment Model</param>
    ///<returns>
    ///Saved employee pension enrollment details
    ///</returns>
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

      EmployeePensionEnrollment? existingEmployeePensionEnrollment = await _employeePensionEnrollmentRepository.
        GetByEmployeeIdAndLastRunIdAsync(employeePensionEnrollmentDto.EmployeeId);
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

    ///<summary>
    ///Get all pension plans
    ///</summary>
    ///<returns>
    ///List of employee pension enrollment details
    ///</returns>
    public async Task<List<EmployeePensionEnrollmentDto>> GetAllEmployeePensionEnrollementsAsync()
    {
      List<EmployeePensionEnrollment> pensionEnrollments = await _employeePensionEnrollmentRepository.GetAllAsync();
      return pensionEnrollments.Select(epe => epe.ToEmployeePensionEnrollmentDto()).ToList();
    }

    ///<summary>
    ///Get employee's latest pension enrollment by employee id
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Emplyoee's latest pension enrollment details
    ///</returns>
    public async Task<EmployeePensionEnrollmentDto?> GetEmployeePensionEnrollementByIdAsync(string employeeId)
    {
      EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository.
        GetByEmployeeIdAndLastRunIdAsync(employeeId) ?? throw new NotFoundException("Employee not found");
      return employeePensionEnrollment.ToEmployeePensionEnrollmentDto();
    }

    ///<summary>
    ///Gets all pension enrollments for a given payroll run id
    ///</summary>
    ///<param name="payrollRunId">Pay roll run Id</param>
    ///<returns>
    ///All pension enrollments for a given payroll run id
    ///</returns>
    public async Task<List<EmployeePensionEnrollmentDto>> GetPensionEnrollementsByPayRollRunIdAsync(int payrollRunId)
    {
      List<EmployeePensionEnrollment> pensionEnrollments = await _employeePensionEnrollmentRepository.GetByPayRollRunIdAsync(payrollRunId);
      return pensionEnrollments.Select(epe => epe.ToEmployeePensionEnrollmentDto()).ToList();
    }

    ///<summary>
    ///Update employee pension enrollment details
    ///</summary>
    ///<param name="employeePensionEnrollmentUpdateDto">Employee's Id</param>
    ///<returns>
    ///Emplyoee's updated pension enrollment details
    ///</returns>
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
        GetByEmployeeIdAndLastRunIdAsync(employeePensionEnrollmentUpdateDto.EmployeeId);

      int oldPensionOptionId = (int)(employeePensionEnrollment?.PensionOptionId);
      if (employeePensionEnrollment != null)
      {
        employeePensionEnrollment.PensionOptionId = employeePensionEnrollmentUpdateDto.PensionOptionId
          ?? employeePensionEnrollment.PensionOptionId;
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

    ///<summary>
    ///Validate employee pension enrollment details before adding or updating pension enrollment
    ///</summary>
    ///<param name="employeePensionEnrollmentDto">Employee's Id</param>
    private static void ValidateAddEmployeesPensionEnrollment(EmployeePensionEnrollmentAddDto employeePensionEnrollmentDto)
    {
      //await CheckIfPensionOptionExists(employeePensionEnrollmentDto.PensionOptionId);
      ValidateEmployeePensionEnrollmentDtos.ValidateAddDto(employeePensionEnrollmentDto);
    }

    ///<summary>
    ///Get pension option percentage for a given pension option id
    ///</summary>
    ///<param name="pensionOptionId">Pension's Id</param>
    ///<returns>
    ///Pension option percentage for a given pension option id
    ///</returns>
    private async Task<decimal> GetEmployeePensionOptionPercentageAsync(int pensionOptionId)
    {
      decimal? employeePensionOption = await _context.PensionOptions.Where(po => po.PensionOptionId == pensionOptionId)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
      return employeePensionOption ?? throw new NotFoundException("Pension option not found");
    }

    ///<summary>
    ///Add employee pension deduction for the employee based on the pension enrollment details
    ///</summary>
    ///<param name="employeePensionEnrollment">Employee Pension Model that was added</param>
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
            IdNumber = existingEmployee.IdNumber,
            Passport = existingEmployee.PassportNumber,
            TaxNumber = existingEmployee.TaxNumber,
            PensionableSalary = existingEmployee.MonthlySalary,
            PensionOptionId = (int)existingEmployee.PensionOptionId,
            PendsionCategoryPercentage = pensionOptionPercentage,
            PensionContribution = ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100))),
            VoluntaryContribution = employeePensionEnrollment.VoluntaryContribution,
            TotalPensionContribution =
            ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100)) + employeePensionEnrollment.VoluntaryContribution),
            EmailAddress = existingEmployee.Email,
            PhysicalAddress = existingEmployee.PhysicalAddress,
            PayrollRunId = currentPayrollRunId.PayrollRunId,
            CreatedDate = employeePensionEnrollment.EffectiveDate,
            IsActive = true
          };


          _ = await _pensionDeductionRepository.AddAsync(employeesPensionDeduction);
        }
      }
    }

    ///<summary>
    ///Update employee pension option and pension deduction details when employee pension option is changed 
    ///in the pension enrollment update process
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<param name="newPensionOptionId">Pension Option Id</param>
    ///<param name="voluntaryContribution">Voluntary contribution</param>
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

    ///<summary>
    ///Get all pension enrollments that are not locked 
    ///</summary>
    ///<returns>
    ///List of pension enrollment details that are not locked
    ///</returns>
    public async Task<List<EmployeePensionEnrollmentDto>> GetPensionEnrollementsNotLocked()
    {
      List<EmployeePensionEnrollment> pensionEnrollments = await _employeePensionEnrollmentRepository.GetEmployeePensionEnrollmentsNotLocked();
      return pensionEnrollments.Select(epe => epe.ToEmployeePensionEnrollmentDto()).ToList();
    }

    ///<summary>
    ///Validation pension contribution
    ///</summary>
    ///<param name="pensionContribution">Pension's contribution</param>
    ///<returns>
    ///Pension contribution that is not above capped the  maximum allowed monthly
    ///</returns>
    private static decimal ValidPensionContribution(decimal pensionContribution)
    {
      return (pensionContribution > MAX_MONTHLYCONTRIBUTION) ? MAX_MONTHLYCONTRIBUTION : pensionContribution;
    }

    ///<summary>
    ///Lock employee pension enrollments for current payroll run
    ///</summary>
    public async Task LockEmployeePensionEnrollmentsAsync()
    {
      PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<EmployeePensionEnrollment> employeePensionEnrollments = await _employeePensionEnrollmentRepository
        .GetByPayRollRunIdAsync(currentPayRollRun.PayrollRunId);

      foreach (EmployeePensionEnrollment enrollment in employeePensionEnrollments.Where(x => !x.IsLocked))
      {
        enrollment.IsLocked = true;
      }

      try
      {
        await _employeePensionEnrollmentRepository.LockEmployeePensionEnrollmentsAsync(employeePensionEnrollments);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error locking employee pension enrollments: {ex}");
      }
    }

    ///<summary>
    ///Roll over employee pension enrollment for employees with pension options for the new payroll run
    ///</summary>
    public async Task RollOverEmloyeePensionEnrollmentAsync()
    {
      PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> employeesWithPensionOption = await _employeeRepository.GetAllEmployeeWithAPensionOption();

      foreach (Employee employee in employeesWithPensionOption)
      {
        if (!employee.IsActive)
        {
          continue;
        }

        EmployeePensionEnrollment? employeeExisitingPensionEnrollment = await _employeePensionEnrollmentRepository.
          GetByEmployeeIdAndLastRunIdAsync(employee.EmployeeId);
        if (employeeExisitingPensionEnrollment != null &&
          (employeeExisitingPensionEnrollment.VoluntaryContribution > decimal.Zero) &&
          employeeExisitingPensionEnrollment.IsVoluntaryContributionPermament != null &&
          employeeExisitingPensionEnrollment.IsVoluntaryContributionPermament == true)
        {
          EmployeePensionEnrollment employeePensionEnrollment = new()
          {
            EmployeeId = employee.EmployeeId,
            PayrollRunId = currentPayRollRun.PayrollRunId,
            PensionOptionId = (int)employee.PensionOptionId,
            StartDate = employee.StartDate,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
            VoluntaryContribution = (bool)employeeExisitingPensionEnrollment.IsVoluntaryContributionPermament ?
              employeeExisitingPensionEnrollment.VoluntaryContribution : 0.00M,
            IsVoluntaryContributionPermament = employeeExisitingPensionEnrollment.IsVoluntaryContributionPermament,
            IsLocked = false,
          };

          if (employeeExisitingPensionEnrollment.PayrollRunId == employeePensionEnrollment.PayrollRunId)
          {
            continue;
          }

          _ = await _employeePensionEnrollmentRepository.AddAsync(employeePensionEnrollment);
        }
        else
        {
          EmployeePensionEnrollment employeePensionEnrollment = new()
          {
            EmployeeId = employee.EmployeeId,
            PayrollRunId = currentPayRollRun.PayrollRunId,
            PensionOptionId = (int)employee.PensionOptionId,
            StartDate = employee.StartDate,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Now),
            IsLocked = false,
          };

          _ = await _employeePensionEnrollmentRepository.AddAsync(employeePensionEnrollment);
        }
      }
    }
  }
}
