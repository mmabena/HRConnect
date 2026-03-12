namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;
  using Quartz;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class PayrollRolloverJob : IJob
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly ApplicationDBContext _context;
    private static readonly int MAX_RUNS = 10;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService, ApplicationDBContext context)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
      _context = context;
    }
    public async Task<PayrollPeriod> RolloverPayrollPeriod(PayrollPeriod? oldPeriod)
    {
      Console.WriteLine($">>>>>>>CREATING NEW PERIOD<<<<<<<"); //Lock period away
      if (oldPeriod != null)
      {
        oldPeriod.IsLocked = true;
        oldPeriod.IsClosed = true;
        await _payrollPeriodService.UpdateAsync(oldPeriod);
      }
      //Instantiate new period and run 
      var newPeriod = new PayrollPeriod
      {
        // StartDate = oldPeriod.StartDate.AddMonths(1),
        // EndDate = oldPeriod.EndDate.AddYears(1)
        StartDate = (oldPeriod?.StartDate ?? DateTime.Now).AddMonths(1),
        EndDate = (oldPeriod?.EndDate ?? DateTime.Now).AddYears(1)
      };
      await _payrollPeriodService.CreatePeriodAsync(newPeriod);

      var newPayrun = new PayrollRun
      {
        PayrollRunNumber = 1,//should be current financial month
        PeriodId = newPeriod.PayrollPeriodId
      };
      newPeriod.Runs.Add(newPayrun);
      // await _payrollPeriodService.UpdateAsync(oldPeriod);
      await _payrollRunRepo.CreatePayrollRunAsync(newPayrun);
      return newPeriod;
    }

    public async Task RolloverPayrollRun(PayrollPeriod payrollPeriod, int runId)
    {
      // Defensive: ensure no duplicate
      if (payrollPeriod.Runs.Any(r => r.PayrollRunNumber == runId))
        throw new InvalidOperationException($"Run {runId} already exists for this period.");

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
    }

    public async Task Execute(IJobExecutionContext context)
    {
      DateTime currentDate = DateTime.Now;
      int runId = ((currentDate.Month + 8) % 12) + 1;
      try
      {
        // var currentPayRun = await _payrollRunRepo.GetPayrunByIdAsync(20);
        var payperiod = await _payrollPeriodService.GetLastPeriodAsync();
        // var currentPayRun = await _payrollRunRepo.GetCurrentRunAsync();
        if (payperiod == null)
        {
          // throw new InvalidDataException("No payroll period found");
          payperiod = await RolloverPayrollPeriod(null);
        }
        // var currentPayRun = payperiod.Runs.OrderByDescending(r => r.PayrollRunId == runId).FirstOrDefault();

        var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();
        int nextRun = currentPayRun == null ? 1 : currentPayRun.PayrollRunNumber + 1; //In production remove this
        Console.WriteLine("=============A JOB Start==========");
        if (currentPayRun == null)
        {
          Console.WriteLine($"-----THIS CURRENT PAY IS EMPTY-----");
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
          }
          //update the current run to implement lock
          await _payrollRunRepo.UpdateRunAsync(currentPayRun);
          //Archive should happen here
          if (currentPayRun.Records.Count > 0)
            await PayrollUtil.WriteExcelAsync(currentPayRun);

          Console.WriteLine($"LOCKED PAYRUN WITH ID {currentPayRun.PayrollRunNumber}");
        }

        if (nextRun > MAX_RUNS)
        {
          Console.WriteLine("----->ROLLING OVER PERIOD<-----");
          payperiod = await RolloverPayrollPeriod(payperiod);
        }
        else
        {

          Console.WriteLine("----->ROLLING OVER RUN<-----");
          await RolloverPayrollRun(payperiod, nextRun);
        }

      }
      catch (InvalidOperationException ex)
      {
        Console.WriteLine($"Invalid Operation on locked entity \n{ex}");
        var jobException = new JobExecutionException();
        throw jobException;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Something else i guess. \n{ex}");
        var jobException = new JobExecutionException();
        throw jobException;
      }
    }
  }
}