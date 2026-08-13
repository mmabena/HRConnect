namespace HRConnect.Api.Utils.Jobs.Payroll
{
  using global::Quartz;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Interfaces.Payroll.Deduction;
  using HRConnect.Api.Interfaces.Payroll.Earning;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Utils;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.DependencyInjection;
  using HRConnect.Api.Services;



  /// <summary>
  /// Payroll Rollover Job class to handle the locking, rolling over and 
  /// payroll run report generation for the current payroll run 
  /// </summary>
  ///<para> 
  /// When a rollover occurs, all records in the current payroll run are locked and frozen. 
  /// This prevents modifications and deletions. On the last payroll run (March 31st the 12 fiscal month) the payroll period automatically rolls over and 
  /// starts the new fiscal year with an empty payroll run with run number 1. On every payroll run roll over, a new fincial report is generated. The report
  /// captures all payroll records in the current run and sorts them into different
  /// excel sheets per type (MedicalAidDeductions, PensionDeduction and  StatutoryContributions have their respective worksheets)
  ///</para>   

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]

  public class PayrollRolloverJob : IJob
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IEmployeePensionEnrollmentService _employeePensionEnrollmentService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IReportsService _reportsService;
    private readonly IBankingDetailService _bankingDetailService;
    // private readonly ICompanyContributionService contributionService;
    private readonly IUserService _userService;
    private readonly IEmployeeService _employeeService;
    private readonly INotificationService _notificationsService;
    private readonly IMedicalAidDependentService _medicalAidDependentService;
    private readonly IMedicalAidDependentNotificationService _dependentNotificationService;
    private static readonly int MAX_RUNS = 12;
    private readonly IEmployeePayrollEarningService _employeePayrollEarningService;
    private readonly IEmployeeDeductionService _employeeDeductionService;
    private readonly IMedicalAidDeductionService _medicalAidDeductionService;
    //This makes mocking and using testing time-related edge cases a lot easier
    private readonly Func<DateTime> _now;

    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService, IServiceProvider serviceProvider,
      IEmployeePensionEnrollmentService employeePensionEnrollmentService,
      IReportsService reportsService, IBankingDetailService bankingDetailService, IUserService userService,
      IEmployeeService employeeService, IMedicalAidDependentNotificationService dependentNotificationService,
      IEmployeePayrollEarningService employeePayrollEarningService,
      IEmployeeDeductionService employeeDeductionService,
      IMedicalAidDependentService medicalAidDependentService, IMedicalAidDeductionService medicalAidDeductionService,
      INotificationService notificationsService, Func<DateTime>? now = null)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
      _reportsService = reportsService;
      _serviceProvider = serviceProvider;
      _employeePensionEnrollmentService = employeePensionEnrollmentService;
      _bankingDetailService = bankingDetailService;
      _userService = userService;
      _medicalAidDependentService = medicalAidDependentService;
      _dependentNotificationService = dependentNotificationService;
      _employeeService = employeeService;
      _medicalAidDeductionService = medicalAidDeductionService;
      _employeeDeductionService = employeeDeductionService;
      _employeePayrollEarningService = employeePayrollEarningService;
      _notificationsService = notificationsService;
      _now = now ?? (() => DateTime.Now);
    }
    /// <summary>
    /// Rolls over to a new period <see cref="PayrollPeriod"/> and creates and new valid payroll run <see cref="PayrollRun"/>  
    /// </summary>
    /// <param name="oldPeriod"></param>
    /// <returns>A new valid payroll period with atleast 1 payroll run</returns>
    public async Task<PayrollPeriod> RolloverPayrollPeriod(PayrollPeriod? oldPeriod)
    {
      Console.WriteLine("==============================================");
      Console.WriteLine("PAYROLLOVERJOB START");
      Console.WriteLine("==============================================");
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

      _ = await _payrollPeriodService.CreatePeriodAsync(newPeriod);

      var newPayrun = new PayrollRun
      {
        PayrollRunNumber = 1,
        PeriodId = newPeriod.PayrollPeriodId,
        PeriodDate = DateTime.Now,
        IsFinalised = false
      };
      newPeriod.Runs.Add(newPayrun);

      _ = await _payrollRunRepo.CreatePayrollRunAsync(newPayrun);

      return newPeriod;
    }
    public async Task ClearPayrollNotifications()
    {
      var users = await _userService.GetAllUsersAsync();

      users = users.FindAll(u => u.Role == UserRole.SuperUser ||
      u.TempRole == UserRole.SuperUser);
      List<string> employeeIds = new();

      foreach (var u in users)
      {
        var e = await _employeeService.GetEmployeeByEmailAsync(u.Email);
        if (e != null)
        {
          employeeIds.Add(e.EmployeeId);
        }
      }
      await _notificationsService.MarkBatchedNotificationsReadByTypeAsync(NotificationType.Payroll,
      employeeIds);
    }
    public async Task RolloverPayrollRun(PayrollPeriod payrollPeriod, int runId)
    {
      PayrollRun newRun = new PayrollRun
      {
        PeriodId = payrollPeriod.PayrollPeriodId,
        PayrollRunNumber = runId,
        IsLocked = false,
        Period = payrollPeriod,
        PeriodDate = DateTime.Now,
        Records = new List<PayrollRecord>()
      };

      payrollPeriod.Runs.Add(newRun);

      await _payrollRunRepo.CreatePayrollRunAsync(newRun);

      await AllocateCompanyContributionsIfNeeded(newRun.PayrollRunId);
    }

    public async Task Execute(IJobExecutionContext context)
    {
      try
      {
        Console.WriteLine("==============================================");
        Console.WriteLine("PAYROLL ROLLOVER JOB START");
        Console.WriteLine($"Current Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("==============================================");

        DateTime currentDate = _now();

        var payperiod = await _payrollPeriodService.GetLastPeriodAsync();

        // If there is no payroll period, create the first one.
        if (payperiod == null)
        {
          payperiod = await RolloverPayrollPeriod(null);
        }

        // Find the current unlocked payroll run.
        var currentPayRun = payperiod.Runs
            .Where(r => !r.IsLocked)
            .OrderByDescending(r => r.PayrollRunNumber)
            .FirstOrDefault();

        // ---------------------------------------------------------
        // FIRST RUN / NO ACTIVE RUN
        // ---------------------------------------------------------
        if (currentPayRun == null)
        {
          int currentFiscalRun = GetFiscalRunNumber(currentDate);

          Console.WriteLine(
              $"No active payroll run found. Creating fiscal payroll run {currentFiscalRun}."
          );

          await RolloverPayrollRun(payperiod, currentFiscalRun);

          // Fetch the newly created run.
          currentPayRun = payperiod.Runs
              .Where(r => !r.IsLocked)
              .OrderByDescending(r => r.PayrollRunNumber)
              .FirstOrDefault();

          if (currentPayRun == null)
          {
            throw new InvalidOperationException(
                "Payroll run was created but could not be retrieved."
            );
          }
        }

        Console.WriteLine(
            $"Current Payroll Run: {currentPayRun.PayrollRunNumber}"
        );

        // ---------------------------------------------------------
        // LOCK PENSION ENROLLMENTS FOR THE ACTIVE RUN
        // ---------------------------------------------------------
        await _employeePensionEnrollmentService
            .LockEmployeePensionEnrollmentsAsync();

        // ---------------------------------------------------------
        // FINALISE CURRENT RUN
        // ---------------------------------------------------------
        if (!currentPayRun.IsFinalised && !currentPayRun.IsLocked)
        {
          currentPayRun.IsFinalised = true;
          currentPayRun.IsLocked = true;
          currentPayRun.FinalisedDate = currentDate;

          foreach (var record in currentPayRun.Records)
          {
            record.IsLocked = true;

            switch (record)
            {
              case PensionDeduction p:
                p.IsActive = false;
                break;

              case MedicalAidDeduction m:
                m.IsActive = false;
                break;

              default:
                break;
            }
          }

          await _payrollRunRepo.UpdateRun(currentPayRun);

          if (currentPayRun.Records.Count > 0)
          {
            await _reportsService.WriteExcelAsync(currentPayRun);
          }
        }

        // ---------------------------------------------------------
        // MEDICAL AID NOTIFICATIONS
        // ---------------------------------------------------------
        Console.WriteLine(
            "====================Checking Medical Aid notifications...===================="
        );

        await _dependentNotificationService
            .NotifyChildrenTurning21Async(currentPayRun);

        Console.WriteLine(
            "====================Converting Medical Aid Dependent ...===================="
        );

        await _medicalAidDependentService
            .ConvertChildrenTurning21Async(currentPayRun);

        // ---------------------------------------------------------
        // DETERMINE NEXT RUN
        // ---------------------------------------------------------
        int nextRun = currentPayRun.PayrollRunNumber + 1;

        Console.WriteLine($"Next Payroll Run: {nextRun}");

        // ---------------------------------------------------------
        // FISCAL YEAR ROLLOVER
        // ---------------------------------------------------------
        if (nextRun > MAX_RUNS)
        {
          Console.WriteLine("Maximum payroll runs reached.");
          Console.WriteLine("Creating a new fiscal payroll period.");

          payperiod = await RolloverPayrollPeriod(payperiod);
        }
        else
        {
          await RolloverPayrollRun(payperiod, nextRun);
        }

        // ---------------------------------------------------------
        // LOCK BANKING DETAILS
        // ---------------------------------------------------------
        await _bankingDetailService.LockAllBankingDetailsAsync();

        // ---------------------------------------------------------
        // ROLLOVER OTHER PAYROLL DATA
        // ---------------------------------------------------------

        await RolloverPensionDeductions();

        Console.WriteLine("====================2====================");

        await _employeePayrollEarningService
            .RollOverEmployeePayrollEarningsAsync();

        Console.WriteLine(
            "========== BEFORE Medical Aid Rollover =========="
        );

        await _medicalAidDeductionService
            .RollOverMedicalAidDeductions();

        Console.WriteLine(
            "========== AFTER Medical Aid Rollover =========="
        );

        Console.WriteLine("====================3====================");

        await _employeeDeductionService
            .RollOverEmployeePayrollEarningsAsync();

        Console.WriteLine("====================4====================");
      }
      catch (InvalidOperationException ex)
      {
        throw new JobExecutionException(ex);
      }
      catch (Exception ex)
      {
        throw new JobExecutionException(ex);
      }
    }
    private async Task AllocateCompanyContributionsIfNeeded(int payrollRunId)
    {
      using var scope = _serviceProvider.CreateScope();
      var companyContributionService = scope.ServiceProvider.GetRequiredService<ICompanyContributionService>();

      bool alreadyAllocated = await companyContributionService.FindAllocatedContribution(payrollRunId);
      if (alreadyAllocated)
        return;

      var allocationService = scope.ServiceProvider
        .GetRequiredService<ICompanyContributionAllocationService>();

      _ = await allocationService.AllocateAsync(payrollRunId);
    }

    ///<summary>
    ///Auxilary function to rollover pay roll records
    ///</summary>
    private async Task RolloverPensionDeductions()
    {
      using IServiceScope pensionDeductionServiceScope = _serviceProvider.CreateScope();

      IPensionDeductionService pensionDeductionService = pensionDeductionServiceScope.ServiceProvider.GetRequiredService<IPensionDeductionService>();

      Task[] tasks =
      [
        pensionDeductionService.PensionDeductionRollover()
      ];

      await Task.WhenAll(tasks);
    }

    private int GetFiscalRunNumber(DateTime date)
    {
      return date.Month switch
      {
        3 => 1,   // March
        4 => 2,   // April
        5 => 3,   // May
        6 => 4,   // June
        7 => 5,   // July
        8 => 6,   // August
        9 => 7,   // September
        10 => 8,  // October
        11 => 9,  // November
        12 => 10, // December
        1 => 11,  // January
        2 => 12,  // February
        _ => throw new ArgumentOutOfRangeException(
            nameof(date),
            "Invalid month."
        )
      };
    }
  }
}