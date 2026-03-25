namespace HRConnect.Api.Utils.Payroll
{
  using global::Quartz;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Repository;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;

  //using Quartz;

  // Prevent multiple of these jobs from running concurrently
  [DisallowConcurrentExecution]
  public class PayrollRolloverJob : IJob
  {
    private readonly IWebHostEnvironment _env;
    private readonly IPayrollPeriodService _payrollPeriodService;
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IEmployeePensionEnrollmentService _employeePensionEnrollmentService;
    private readonly IPensionDeductionService _pensionDeductionService;
    private readonly IServiceProvider _serviceProvider;
    private static readonly int MAX_RUNS = 10;
    public PayrollRolloverJob(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService, IServiceProvider serviceProvider,
      IEmployeePensionEnrollmentService employeePensionEnrollmentService, IPensionDeductionService pensionDeductionService,
      IWebHostEnvironment env)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
      _env = env;
      _serviceProvider = serviceProvider;
      _employeePensionEnrollmentService = employeePensionEnrollmentService;
      _pensionDeductionService = pensionDeductionService;
    }
    /// <summary>
    /// Rolls over to a new period <see cref="PayrollPeriod"/> and creates and new valid payroll run <seealso cref="PayrollRun"/>  
    /// </summary>
    /// <param name="oldPeriod"></param>
    /// <returns>A new valid pauyroll period with atleast 1 payroll run</returns>
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
      Console.WriteLine($"ADDED RUN TO PERIOD\n{payrollPeriod.Runs.Count}");
    }

    public async Task Execute(IJobExecutionContext context)
    {
      await _employeePensionEnrollmentService.LockEmployeePensionEnrollmentsAsync();
      DateTime currentDate = DateTime.Now;
      int runId = ((currentDate.Month + 8) % 12) + 1;
      try
      {
        var payperiod = await _payrollPeriodService.GetLastPeriodAsync();

        if (payperiod == null)
        {
          payperiod = await RolloverPayrollPeriod(null);
        }

        var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();
        int nextRun = currentPayRun == null ? 1 : currentPayRun.PayrollRunNumber + 1; //In production remove this

        if (currentPayRun == null)
        {
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

            if (record is PensionDeduction pensionDeduction)
            {
              pensionDeduction.IsActive = false;
            }
          }
          //update the current run to implement lock
          await _payrollRunRepo.UpdateRun(currentPayRun);

          if (currentPayRun.Records.Count > 0)
            await PayrollUtil.WriteExcelAsync(currentPayRun, _env.ContentRootPath);
        }

        Console.WriteLine($"NEXT RUN========{nextRun}");
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

      //
      //await _pensionDeductionService.PensionDeductionRollover();
      /*await Task.WhenAll(
        _employeePensionEnrollmentService.RollOverEmloyeePensionEnrollmentAsync(),
        _pensionDeductionService.PensionDeductionRollover()
      );*/
      await _employeePensionEnrollmentService.RollOverEmloyeePensionEnrollmentAsync();
      await RolloverPayrollDeductions();
    }

    private async Task RolloverPayrollDeductions()
    {
      using IServiceScope pensionDeductionServiceScope = _serviceProvider.CreateScope();

      IPensionDeductionService pensionDeductionService = pensionDeductionServiceScope.ServiceProvider.GetRequiredService<IPensionDeductionService>();

      var tasks = new[]
      {
        pensionDeductionService.PensionDeductionRollover()
      };

      await Task.WhenAll(tasks);
    }
  }
}