namespace HRConnect.Api.Utils.Payroll
{
  using Interfaces;
  using Models.Payroll;

  public class PayrollInit
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IPayrollRunService _payrollRunService;
    public PayrollInit(IPayrollPeriodService payrollPeriodService, IPayrollRunRepository payrollRunRepository, IPayrollRunService payrollRunService)
    {
      _payrollPeriodService = payrollPeriodService;
      _payrollRunRepo = payrollRunRepository;
      _payrollRunService = payrollRunService;
    }

    /// <summary>
    /// Intitialies a valid active payroll period <see cref="PayrollPeriod"/>
    /// and adds new unlocked payroll run <see cref="PayrollRun"/>
    /// to the period's collection
    /// </summary>
    /// <remarks>
    /// This method is called in the applications entry point. Since only 1 payroll period and run
    /// can be active at a time, let the application handle this automatically. No user input is required of allowed
    /// </remark>
    public async Task InitialisePayrollPeriod()
    {
      var payperiod = await _payrollPeriodService.GetLastPeriodAsync(); // in production remove this
      await _payrollRunService.LockAllOlderPayrollRuns();
      if (payperiod == null)
      {
        payperiod = new PayrollPeriod();
        await _payrollPeriodService.CreatePeriodAsync(payperiod);
      }

      int runNumber = PayrollUtil.SetPayrunNumber();
      //Do the same thing for the period
      var runExists = await _payrollRunRepo.GetPayrunByRunNumberAsync(runNumber);

      if (runExists == null)
      {
        PayrollRun newRun = new PayrollRun
        {
          PeriodId = payperiod.PayrollPeriodId,
          PayrollRunNumber = runNumber,
          IsLocked = false,
          Period = payperiod,
          PeriodDate = DateTime.Now,
          Records = new List<PayrollRecord>()
        };
        payperiod.Runs.Add(newRun);
        await _payrollRunRepo.CreatePayrollRunAsync(newRun);
      }

    }
  }
}