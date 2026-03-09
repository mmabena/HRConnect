namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;

  public class PayrollInit
  {
    private readonly IPayrollPeriodRepository _payrollPeriodRepo;
    private readonly IPayrollRunRepository _payrollRunRepo;
    public PayrollInit(IPayrollPeriodRepository payrollPeriodRepository, IPayrollRunRepository payrollRunRepository)
    {
      _payrollPeriodRepo = payrollPeriodRepository;
      _payrollRunRepo = payrollRunRepository;
    }

    /// <summary>
    /// Get the current tax month 
    /// 4 -> April. The first month of our financial period
    /// 3-> March. The last month of our financial period
    /// <summary>
    public (DateTime start, DateTime end) GetCurrectFinancialPeriod()
    {
      DateTime today = DateTime.Now;
      int startYear = 0;
      if (today.Month >= 4)
      {
        //April -> December: Current financial year
        startYear = today.Year;
      }
      else
      {
        // Jan -> March: Previoud financial year
        startYear = today.Year - 1;
      }
      //1st of April
      DateTime start = new DateTime(startYear, 4, 1);
      DateTime end = new DateTime(startYear + 1, 3, 31);
      return (start, end);
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
      //Get the current payroll period
      (DateTime start, DateTime end) = GetCurrectFinancialPeriod();

      var payperiod = await _payrollPeriodRepo.GetActivePeriod(DateTime.Now);
      if (payperiod == null)
      {
        PayrollPeriod p = new PayrollPeriod
        {
          PayrollPeriodId = Guid.NewGuid(),
          StartDate = start,
          EndDate = end,
          IsLocked = false,
          IsClosed = false,
          Runs = new List<PayrollRun>()
        };
        //create the period after saving the run
        Console.WriteLine($"CREATED A PAYROLL PERIOD");
        await _payrollPeriodRepo.CreatePeriodAsync(p);
      }
      int run = GetPayrunNumber(DateTime.Now);
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
          Records = new List<PayrollRecord>()
        };
        // payperiod.Runs.Add(newRun);

        await _payrollRunRepo.CreatePayrollRunAsync(newRun);

        Console.WriteLine($"CREATED A PAYROLL RUN");
      }

    }
  }
}