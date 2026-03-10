namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Mappers.Payroll;
  using HRConnect.Api.Utils.Payroll;

  public class PayrollRunService : IPayrollRunService
  {
    private readonly IPayrollRunRepository _payrollRunRepo;
    // private readonly PayrollInit _payrollInit;
    public PayrollRunService(IPayrollRunRepository payrollRunRepo)
    {
      _payrollRunRepo = payrollRunRepo;
      // _payrollInit = new PayrollInit();
    }
    public async Task<PayrollRunDto?> GetPayrunByIdAsync(int id)
    {
      var payrun = await _payrollRunRepo.GetPayrunByIdAsync(id);
      if (payrun == null)
        return null;
      return payrun.ToPayrollRunDto();
    }
    public async Task<IEnumerable<PayrollRunDto>> GetAllPayruns()
    {
      var payruns = await _payrollRunRepo.GetAllPayruns();
      return payruns.Select(p => p.ToPayrollRunDto()).ToList();
    }
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    public async Task<PayrollRunDto> CreatePayrollRunAsync(PayrollRun payrollRun)
    {
      // var exists = await _payrollRunRepo.GetCurrentRunAsync();

      // if (exists != null)
      //   return exists.ToPayrollRunDto();

      DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
      Console.WriteLine($"Creating payroll run for month: {currentMonth}");
      //maps current financial month to 1-12
      payrollRun.IsLocked = false;
      payrollRun.IsFinalised = false;
      payrollRun.PeriodDate = currentMonth;

      await _payrollRunRepo.CreatePayrollRunAsync(payrollRun);
      return payrollRun.ToPayrollRunDto();
    }
    public async Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime)
    {
      throw new NotImplementedException();
    }
    public async Task<PayrollRun> GetCurrentRunAsync()
    {
      var payrun = await _payrollRunRepo.GetCurrentRunAsync();

      if (payrun == null)
        return null;
      return payrun;
    }

    public async Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord)
    {
      var currentRun = await _payrollRunRepo.GetCurrentRunAsync();
      if (currentRun == null)
        return;
      currentRun.Records!.Add(payrollRecord); //records may be null
    }
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      await _payrollRunRepo.UpdateRunAsync(payrollRun);
    }

  }
}