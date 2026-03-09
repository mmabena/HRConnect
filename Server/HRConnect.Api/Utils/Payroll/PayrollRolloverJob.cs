namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;
  using Quartz;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class PayrollRolloverJob : IJob
  {
    //payrollRun repo        
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IPayrollPeriodRepository _payrollPeriodRepo;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo,
        IPayrollPeriodRepository payrollPeriodRepo)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodRepo = payrollPeriodRepo;
    }

    ///Solely for testing roll over
    private async Task<int> GetNextRunID()
    {
      var lastRun = await _payrollRunRepo.GetLastPayrun();

      if (lastRun == null)
        return 1;
      return lastRun.PayrollRunId + 1;
    }
    //
    public async Task Execute(IJobExecutionContext context)
    {
      DateTime currentDate = DateTime.Now;
      int runId = ((currentDate.Month + 8) % 12) + 1;
      try
      {
        var currentPayRun = await _payrollRunRepo.GetPayrunByIdAsync(20);
        // var currentPayRun = await _payrollRunRepo.GetCurrentRunAsync();

        Console.WriteLine("=============A JOB Start==========");
        if (currentPayRun == null)
          return;

        //Finalise and lock a run isnt't finalised and still running
        if (!currentPayRun.IsFinalised && !currentPayRun.IsLocked)
        //      && DateTime.Now >= currentPayRun.PeriodDate)
        {
          Console.WriteLine("Trying to finalise payroll run for month: " + currentPayRun.PayrollRunId);
          Console.WriteLine($"WITH PeriodDate ==> {currentPayRun.PeriodDate}");
          //Finalise a payroll run and add timestamp
          currentPayRun.IsFinalised = true;
          currentPayRun.IsLocked = true;
          currentPayRun.FinalisedDate = DateTime.Now;

          //update the current run to implement lock
          await _payrollRunRepo.UpdateRunAsync(currentPayRun);
          Console.WriteLine($"LOCKED PAYRUN WITH ID {currentPayRun.PayrollRunId}");
        }


        var payperiod = await _payrollPeriodRepo.GetActivePeriod(DateTime.Now)!;
        int nextRun = currentPayRun.PayrollRunId + 1;
        var existingRun = await _payrollRunRepo.GetPayrunByIdAsync(nextRun);
        if (payperiod != null)
        {
          PayrollRun newRun = new PayrollRun
          {
            PeriodId = payperiod.PayrollPeriodId,
            PayrollRunId = nextRun,//GetPayrunNumber(DateTime.Now),
            IsLocked = false,
            Period = payperiod,
            Records = new List<PayrollRecord>()
          };

          // payperiod.Runs.Add(newRun);
          await _payrollRunRepo.CreatePayrollRunAsync(newRun);
        }
        Console.WriteLine($"CREATED A PAYROLL RUN");

      }
      catch (InvalidOperationException ex)
      {
        Console.WriteLine($"Invalid Operation on locked entity \n{ex}");
        var jobException = new JobExecutionException();
        throw jobException;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Something else i guess. {ex}");
        var jobException = new JobExecutionException();
        throw jobException;
      }
    }
  }
}