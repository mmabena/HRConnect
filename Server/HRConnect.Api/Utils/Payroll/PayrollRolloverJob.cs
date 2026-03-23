namespace HRConnect.Api.Utils.Payroll
{
  using global::Quartz;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Repository;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;

  //using Quartz;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class PayrollRolloverJob : IJob
  {
    private readonly IWebHostEnvironment _env;
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository;
    private readonly IPensionDeductionRepository _pensionDeductionRepository;
    private readonly IPayrollRunService _payrollRunService;
    private readonly ApplicationDBContext _context;
    private static readonly int MAX_RUNS = 10;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService,
      IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository, IEmployeeRepository employeeRepository,
      IPensionDeductionRepository pensionDeductionRepository, ApplicationDBContext context, IPayrollRunService payrollRunService
      , IWebHostEnvironment env)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
      _env = env;
      _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
      _employeeRepository = employeeRepository;
      _pensionDeductionRepository = pensionDeductionRepository;
      _payrollRunService = payrollRunService;
      _context = context;
    }
    /// <summary>
    /// Rolls over to a new period <see cref="PayrollPeriod"/> and creates and new valid payroll run <seealso cref="PayrollRun"/>  
    /// </summary>
    /// <param name="oldPeriod"></param>
    /// <returns>A new valid pauyroll period with atleast 1 payroll run</returns>
    public async Task<PayrollPeriod> RolloverPayrollPeriod(PayrollPeriod? oldPeriod)
    {
      if (oldPeriod != null)
      {
        oldPeriod.IsLocked = true;
        oldPeriod.IsClosed = true;
        await _payrollPeriodService.UpdateAsync(oldPeriod);
      }

      var newPeriod = new PayrollPeriod
      {
        StartDate = (oldPeriod?.StartDate ?? DateTime.Now).AddMonths(1),
        EndDate = (oldPeriod?.EndDate ?? DateTime.Now).AddYears(1)
      };

      await _payrollPeriodService.CreatePeriodAsync(newPeriod);
      var newPayrun = new PayrollRun
      {
        PayrollRunNumber = 1,//PayrollUtil.SetPayrunNumber(),
        PeriodId = newPeriod.PayrollPeriodId,
        PeriodDate = DateTime.Now,
        IsFinalised = false
      };
      newPeriod.Runs.Add(newPayrun);

      await _payrollRunRepo.CreatePayrollRunAsync(newPayrun);
      return newPeriod;
    }

    public async Task RolloverPayrollRun(PayrollPeriod payrollPeriod, int runId)
    {
      Console.WriteLine($"====>Existing Run has RUNID {runId} <====");
      PayrollRun newRun = new PayrollRun
      {
        PeriodId = payrollPeriod.PayrollPeriodId,
        PayrollRunNumber = runId,
        IsLocked = false,
        // Period = payrollPeriod,
        PeriodDate = DateTime.Now,
        Records = new List<PayrollRecord>()
      };

      payrollPeriod.Runs.Add(newRun);
      await _payrollRunRepo.CreatePayrollRunAsync(newRun);
      Console.WriteLine($"ADDED RUN TO PERIOD\n{payrollPeriod.Runs.Count}");
    }

    public async Task Execute(IJobExecutionContext context)
    {
      await LockEmployeePensionEnrollmentsAsync();
      DateTime currentDate = DateTime.Now;
      int runId = ((currentDate.Month + 8) % 12) + 1;
      try
      {
        var payperiod = await _payrollPeriodService.GetLastPeriodAsync();

        if (payperiod == null)
        {
          payperiod = await RolloverPayrollPeriod(null);
        }

        var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();
        int nextRun = currentPayRun == null ? 1 : currentPayRun.PayrollRunNumber + 1; //In production remove this

        if (currentPayRun == null)
        {
          await RolloverPayrollRun(payperiod, nextRun);
          return; //avoiding null dereference warnings
        }

        //Finalise and lock a run isnt't finalised and still running
        if (!currentPayRun.IsFinalised && !currentPayRun.IsLocked)
        {
          Console.WriteLine("Trying to finalise payroll run for month: " + currentPayRun.PayrollRunNumber);
          currentPayRun.IsFinalised = true;
          currentPayRun.IsLocked = true;
          currentPayRun.FinalisedDate = DateTime.Now;

          foreach (var record in currentPayRun.Records)
          {
            record.IsLocked = true;

            if (record is PensionDeduction pensionDeduction)
            {
              pensionDeduction.IsActive = false;
            }
          }
          //update the current run to implement lock
          await _payrollRunRepo.UpdateRun(currentPayRun);

          if (currentPayRun.Records.Count > 0)
            await PayrollUtil.WriteExcelAsync(currentPayRun, _env.ContentRootPath);
        }

        Console.WriteLine($"NEXT RUN========{nextRun}");
        if (nextRun > MAX_RUNS)
        {
          payperiod = await RolloverPayrollPeriod(payperiod);
        }
        else
        {
          await RolloverPayrollRun(payperiod, nextRun);
        }
      }
      catch (InvalidOperationException ex)
      {
        Console.WriteLine($"Invalid Operation on locked entity \n{ex}");
        var jobException = new JobExecutionException();
        // throw jobException;
      }
      catch (Exception ex)
      {
        var jobException = new JobExecutionException(ex);
        throw jobException;
      }

      await RollOverEmloyeePensionEnrollmentAsync();
      await PensionDeductionRollover();
    }

    private async Task LockEmployeePensionEnrollmentsAsync()
    {
      PayrollRun? currentPayRollRun = await _payrollRunRepo.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
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

    private async Task RollOverEmloyeePensionEnrollmentAsync()
    {
      PayrollRun? currentPayRollRun = await _payrollRunRepo.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> employeesWithPensionOption = await _employeeRepository.GetAllEmployeeWithAPensionOption();

      foreach (Employee employee in employeesWithPensionOption)
      {
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

    private async Task PensionDeductionRollover()
    {
      List<EmployeePensionEnrollment> employeePensionEnrollments = await _employeePensionEnrollmentRepository.GetEmployeePensionEnrollmentsNotLocked();

      foreach (EmployeePensionEnrollment enrollment in employeePensionEnrollments)
      {
        Employee? employee = await _employeeRepository.GetEmployeeByIdAsync(enrollment.EmployeeId);
        if (employee != null)
        {
          decimal pensionCategoryPercentage = await _context.PensionOptions
          .Where(po => po.PensionOptionId == employee.PensionOptionId)
          .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();

          PensionDeduction pensionDeduction = new()
          {
            EmployeeId = enrollment.EmployeeId,
            FirstName = employee.Name,
            LastName = employee.Surname,
            DateJoinedCompany = employee.StartDate,
            IDNumber = employee.IdNumber,
            Passport = employee.PassportNumber,
            TaxNumber = employee.TaxNumber,
            PensionableSalary = employee.MonthlySalary,
            PensionOptionId = enrollment.PensionOptionId,
            PendsionCategoryPercentage = pensionCategoryPercentage,
            PensionContribution = Math.Round(employee.MonthlySalary * (pensionCategoryPercentage / 100)),
            VoluntaryContribution = enrollment.VoluntaryContribution,
            EmailAddress = employee.Email,
            PhyscialAddress = employee.PhysicalAddress,
            CreatedDate = DateOnly.FromDateTime(DateTime.Now),
            PayrollRunId = enrollment.PayrollRunId,
            IsActive = true
          };

          PensionDeduction? existingEmployeePensionDeduction = await _pensionDeductionRepository
            .GetByEmployeeIdAndLastRunIdAsync(pensionDeduction.EmployeeId, pensionDeduction.PayrollRunId);

          if (existingEmployeePensionDeduction == null)
          {
            await _payrollRunService.AddRecordToCurrentRunAsync(pensionDeduction, enrollment.EmployeeId);
            _ = await _pensionDeductionRepository.AddAsync(pensionDeduction);
          }
        }
      }
    }

  }
}