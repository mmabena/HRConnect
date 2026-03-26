namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;
  using Quartz;

  // Prevent multiple of these jobs from running concurrently

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
    private readonly IReportsService _reportsService;
    private static readonly int MAX_RUNS = 12;

    //This makes mocking and testing time a lot easier
    private readonly Func<DateTime> _now;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService,
        IReportsService reportsService, Func<DateTime> now = null

        )
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
      _reportsService = reportsService;
      _now = now ?? (() => DateTime.Now);
    }
    /// <summary>
    /// Rolls over to a new period <see cref="PayrollPeriod"/> and creates and new valid payroll run <see cref="PayrollRun"/>  
    /// </summary>
    /// <param name="oldPeriod"></param>
    /// <returns>A new valid payroll period with atleast 1 payroll run</returns>
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
    }

    public async Task Execute(IJobExecutionContext context)
    {
      DateTime currentDate = DateTime.Now;
      int runId = ((currentDate.Month + 8) % 12) + 1;

      //   if (currentDate.Date !=
      // new DateTime(currentDate.Year, currentDate.Month,
      // DateTime.DaysInMonth(currentDate.Year, currentDate.Month)))
      //   {
      //     Console.WriteLine("Safe Guard Doing It's Job");
      //     return;
      //   }

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

        //Finalise and lock a run if it isn't finalised and is still running
        if (!currentPayRun.IsFinalised && !currentPayRun.IsLocked)
        {
          currentPayRun.IsFinalised = true;
          currentPayRun.IsLocked = true;
          currentPayRun.FinalisedDate = DateTime.Now;

          foreach (var record in currentPayRun.Records)
          {
            record.IsLocked = true;
          }
          //update the current run to implement lock
          await _payrollRunRepo.UpdateRun(currentPayRun);

          if (currentPayRun.Records.Count > 0)
            await _reportsService.WriteExcelAsync(currentPayRun);
        }

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
    }
  }
}