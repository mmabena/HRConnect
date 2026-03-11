namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;
  using Quartz;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class PayrollRolloverJob : IJob
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    private static readonly int MAX_RUNS = 12;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
    }
    public async Task RolloverPayrollPeriod(PayrollPeriod oldPeriod)
    {
      Console.WriteLine($">>>>>>>CREATING NEW PERIOD<<<<<<<"); //Lock period away
      oldPeriod.IsLocked = true;
      oldPeriod.IsClosed = true;
      await _payrollPeriodService.UpdateAsync(oldPeriod);
      //Instantiate new period and run 
      var newPeriod = new PayrollPeriod
      {
        StartDate = oldPeriod.StartDate.AddDays(1),
        EndDate = oldPeriod.EndDate.AddYears(1)
      };
      Console.WriteLine($"++++++OLD PAYROLL PERIOD DATE\n {oldPeriod.StartDate}-->{oldPeriod.EndDate} +++++++");
      await _payrollPeriodService.CreatePeriodAsync(newPeriod);

      var newPayrun = new PayrollRun
      {
        PayrollRunId = 1,//should be current financial month
        PeriodId = newPeriod.PayrollPeriodId
      };
      newPeriod.Runs.Add(newPayrun);
      await _payrollPeriodService.UpdateAsync(oldPeriod);
      await _payrollRunRepo.CreatePayrollRunAsync(newPayrun);
    }

    public async Task RolloverPayrollRun(PayrollPeriod payrollPeriod, int runId)
    {
      Console.WriteLine($"====>Existing Run has RUNID {runId} <====");
      PayrollRun newRun = new PayrollRun
      {
        PeriodId = payrollPeriod.PayrollPeriodId,
        PayrollRunId = runId,//GetPayrunNumber(DateTime.Now),
        IsLocked = false,
        Period = payrollPeriod,
        PeriodDate = DateTime.Now,
        // Records = new List<PayrollRecord>()
      };

      payrollPeriod.Runs.Add(newRun);
      await _payrollRunRepo.CreatePayrollRunAsync(newRun);
      Console.WriteLine($"ADDED RUN TO PERIOD\n{payrollPeriod.Runs.Count}");
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
          throw new InvalidDataException("No payroll period found");
        // var currentPayRun = payperiod.Runs.OrderByDescending(r => r.PayrollRunId == runId).FirstOrDefault();

        var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunId).FirstOrDefault();
        int nextRun = currentPayRun == null ? 2 : currentPayRun.PayrollRunId + 1; //In production remove this
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
          Console.WriteLine("Trying to finalise payroll run for month: " + currentPayRun.PayrollRunId);
          currentPayRun.IsFinalised = true;
          currentPayRun.IsLocked = true;
          currentPayRun.FinalisedDate = DateTime.Now;

          foreach (var record in currentPayRun.Records)
          {
            record.IsLocked = true;
          }
          //update the current run to implement lock
          await _payrollRunRepo.UpdateRunAsync(currentPayRun);
          Console.WriteLine($"LOCKED PAYRUN WITH ID {currentPayRun.PayrollRunId}");
        }

        Console.WriteLine($"NEXT RUN========{nextRun}");
        if (nextRun > MAX_RUNS)
        {
          Console.WriteLine("----->ROLLING OVER PERIOD<-----");
          await RolloverPayrollPeriod(payperiod);
        }
        else
        {

          Console.WriteLine("----->ROLLING OVER RUN<-----");
          await RolloverPayrollRun(payperiod, nextRun);
        }
        if (payperiod != null)
        {
          if (nextRun % 2 == 0)
          {
            // payperiod.IsClosed = true;
            // payperiod.IsLocked = true;

            // await _payrollPeriodService.UpdateAsync(payperiod);

            // //reassign payperiod to a new period
            // payperiod = new PayrollPeriod();
            // await _payrollPeriodRepo.CreatePeriodAsync(payperiod);


            // Console.WriteLine("====>Existing Run is not null<====");
            // PayrollRun newRun = new PayrollRun
            // {
            //   PeriodId = payperiod.PayrollPeriodId,
            //   PayrollRunId = 1,//GetPayrunNumber(DateTime.Now),
            //   IsLocked = false,
            //   Period = payperiod,
            //   PeriodDate = DateTime.Now,
            //   // Records = new List<PayrollRecord>()
            // };
            // payperiod.Runs.Add(newRun);
            // Console.WriteLine($"ADDED RUN TO PERIOD\n{payperiod.Runs.Count}");
            // await _payrollPeriodService.UpdateAsync(payperiod);
            // await _payrollRunRepo.CreatePayrollRunAsync(newRun);
          }

          // var existingRun = await _payrollRunRepo.GetPayrunByIdAsync(nextRun);
          // if (existingRun == null)
          // {
          //   Console.WriteLine("====>Existing Run is not null<====");
          //   PayrollRun newRun = new PayrollRun
          //   {
          //     PeriodId = payperiod.PayrollPeriodId,
          //     PayrollRunId = nextRun,//GetPayrunNumber(DateTime.Now),
          //     IsLocked = false,
          //     Period = payperiod,
          //     PeriodDate = DateTime.Now,
          //     // Records = new List<PayrollRecord>()
          //   };
          //   payperiod.Runs.Add(newRun);
          //   Console.WriteLine($"ADDED RUN TO PERIOD\n{payperiod.Runs.Count}");
          //   await _payrollPeriodService.UpdateAsync(payperiod);
          //   await _payrollRunRepo.CreatePayrollRunAsync(newRun);
          // }
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