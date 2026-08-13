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
      _ = await _payrollRunRepo.CreatePayrollRunAsync(newRun);


   await AllocateCompanyContributionsIfNeeded(newRun.PayrollRunId);

    }

    public async Task Execute(IJobExecutionContext context)
    {
          Console.WriteLine("==============================================");
    Console.WriteLine("PAYROLL ROLLOVER JOB STARTED");
    Console.WriteLine($"Time: {_now()}");
    Console.WriteLine("==============================================");
      // await _employeePensionEnrollmentService.LockEmployeePensionEnrollmentsAsync();
      DateTime currentDate = DateTime.Now;
      int runId = ((currentDate.Month + 8) % 12) + 1;

      // if (currentDate.Date !=
      //   new DateTime(currentDate.Year, currentDate.Month,
      //   DateTime.DaysInMonth(currentDate.Year, currentDate.Month)))
      // {
      //   return;
      // }

      try
      {
        var payperiod = await _payrollPeriodService.GetLastPeriodAsync();

        if (payperiod == null)
        {
          payperiod = await RolloverPayrollPeriod(null);
        }

        var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();
        int nextRun = currentPayRun == null ? 1 : currentPayRun.PayrollRunNumber + 1;

        if (currentPayRun == null)
        {
          await RolloverPayrollRun(payperiod, nextRun);
          return;
        }

        if (!currentPayRun.IsFinalised && !currentPayRun.IsLocked)
        {
          currentPayRun.IsFinalised = true;
          currentPayRun.IsLocked = true;
          currentPayRun.FinalisedDate = DateTime.Now;

          foreach (var record in currentPayRun.Records)
          {
            record.IsLocked = true;

            //By default every other record that should not be marked as inactive
            // and  is only locked and reported
            switch (record)
            {
              case PensionDeduction p:
                p.IsActive = false;
                break;
              case MedicalAidDeduction m:
                m.IsActive = false;
                break;
              default:
                continue;
            }
          }

          await _payrollRunRepo.UpdateRun(currentPayRun);

          if (currentPayRun.Records.Count > 0)
            await _reportsService.WriteExcelAsync(currentPayRun);
        }

        Console.WriteLine("====================Checking Medical Aid notifications...====================");
        await _dependentNotificationService.NotifyChildrenTurning21Async(currentPayRun);


        Console.WriteLine("====================Converting Medical Aid Dependent ...====================");
        await _medicalAidDependentService.ConvertChildrenTurning21Async(currentPayRun);

        if (nextRun > MAX_RUNS)
        {
          payperiod = await RolloverPayrollPeriod(payperiod);
        }
        else
        {
          await RolloverPayrollRun(payperiod, nextRun);
        }

        // Lock all banking details on payroll rollover to prevent changes to banking details while payroll runs are active
        await _bankingDetailService.LockAllBankingDetailsAsync();


        // await ClearPayrollNotifications();

      }
      catch (InvalidOperationException ex)
      {
        var jobException = new JobExecutionException(ex);
        throw jobException;
      }
      catch (Exception ex)
      {
        var jobException = new JobExecutionException(ex);
        throw jobException;
      }
      // Console.WriteLine("====================1====================");
      // await _employeePensionEnrollmentService.RollOverEmloyeePensionEnrollmentAsync();
      await RolloverPensionDeductions();
      Console.WriteLine("====================2====================");
      await _employeePayrollEarningService.RollOverEmployeePayrollEarningsAsync();

      Console.WriteLine("========== BEFORE Medical Aid Rollover ==========");


      await _medicalAidDeductionService.RollOverMedicalAidDeductions();

      Console.WriteLine("========== AFTER Medical Aid Rollover ==========");
      Console.WriteLine("====================3====================");
      await _employeeDeductionService.RollOverEmployeePayrollEarningsAsync();
      Console.WriteLine("====================4====================");

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
  }
}
