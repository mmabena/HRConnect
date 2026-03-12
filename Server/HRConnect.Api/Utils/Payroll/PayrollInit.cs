namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;

  public class PayrollInit
  {
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    public PayrollInit(IPayrollPeriodService payrollPeriodService, IPayrollRunRepository payrollRunRepository)
    {
      _payrollPeriodService = payrollPeriodService;
      _payrollRunRepo = payrollRunRepository;
    }


    /// <summary >
    /// Helper function to get the current payroll run number based on current date
    /// <para name="currentDate">The date used to find the desired run <param> 
    /// <summary >
    public int GetPayrunNumber(DateTime currentDate)
    {
      return ((currentDate.Month + 8) % 12) + 1;
    }

    public async Task InitialisePayrollPeriod()
    {

      // var payperiod = await _payrollPeriodService.GetActivePeriod(DateTime.Now);
      var payperiod = await _payrollPeriodService.GetLastPeriodAsync(); // in production remove this
      if (payperiod == null)
      {
        payperiod = new PayrollPeriod();
        await _payrollPeriodService.CreatePeriodAsync(payperiod);
        // payperiod = new PayrollPeriod
        // {
        //   PayrollPeriodId = Guid.NewGuid(),
        //   StartDate = start,
        //   EndDate = end,
        //   IsLocked = false,
        //   IsClosed = false,
        //   Runs = new List<PayrollRun>()
        // };
        // //create the period after saving the run
        // Console.WriteLine($"=======>CREATED A PAYROLL PERIOD<=======");
        // await _payrollPeriodService.CreatePeriodAsync(payperiod);
      }
      int run = 1;//GetPayrunNumber(DateTime.Now);
      //Do the same thing for the period
      var runExists = await _payrollRunRepo.GetPayrunByIdAsync(run);
      if (runExists == null)
      {
        PayrollRun newRun = new PayrollRun
        {
          PeriodId = payperiod.PayrollPeriodId,
          PayrollRunId = run,//GetPayrunNumber(DateTime.Now),
          IsLocked = false,
          Period = payperiod,
          PeriodDate = DateTime.Now,
          Records = new List<PayrollRecord>()
        };
        payperiod.Runs.Add(newRun);

        // await _payrollPeriodService.UpdateAsync(payperiod);
        await _payrollRunRepo.CreatePayrollRunAsync(newRun);

        Console.WriteLine($"=======>CREATED A PAYROLL RUN<=======");
      }

    }
  }
}