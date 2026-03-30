namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Interfaces;
  using Quartz;
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

      if (currentPayRun == null)
        return;

      //Finalise and lock a run isnt't finalised and still running
      if (!currentPayRun.IsFinalised && DateTime.UtcNow >= currentPayRun.PeriodDate)
      {
        //Finalise a payroll run and add timestamp
        currentPayRun.IsFinalised = true;
        currentPayRun.FinalisedDate = DateTime.UtcNow;

        //update the current run to implement lock
        await _payrollRunRepo.UpdateRunAsync(currentPayRun);
      }
    }
  }
}