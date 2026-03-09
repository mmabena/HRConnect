namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Interfaces;
  using Quartz;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class PayrollRolloverJob : IJob
  {
    //payrollRun repo        
    private readonly IPayrollRunRepository _payrollRunRepo;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo)
    {
      _payrollRunRepo = payrollRunRepo;
    }

    public async Task Execute(IJobExecutionContext context)
    {
      var currentPayRun = await _payrollRunRepo.GetCurrentRunAsync();
      Console.WriteLine("=============A JOB RAN==========");
      if (currentPayRun == null)
        return;

      //Finalise and lock a run isnt't finalised and still running
      if (!currentPayRun.IsFinalised && !currentPayRun.IsLocked
      && DateTime.UtcNow >= currentPayRun.PeriodDate)
      {
        //Finalise a payroll run and add timestamp
        currentPayRun.IsFinalised = true;
        currentPayRun.IsLocked = true;
        currentPayRun.FinalisedDate = DateTime.UtcNow;

        //update the current run to implement lock
        await _payrollRunRepo.UpdateRunAsync(currentPayRun);
      }
    }
  }
}