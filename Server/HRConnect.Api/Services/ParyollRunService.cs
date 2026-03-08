namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Mappers.Payroll;

  public class ParyollRunService
  {
    private readonly IPayrollRunRepository _payrollRunRepo;
    public ParyollRunService(IPayrollRunRepository payrollRunRepo)
    {
      _payrollRunRepo = payrollRunRepo;
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
      DateTime currentDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

      var exists = await _payrollRunRepo.GetCurrentRunAsync();
      if (exists != null)//there's no current run for this month, create one
        return exists.ToPayrollRunDto();

      //maps from 1-12 based on finacial months
      int runId = ((currentDate.Month + 8) % 12) + 1;
      payrollRun.PayrollRunId = runId;
      payrollRun.IsFinalised = false;
      payrollRun.PeriodDate = currentDate;

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
      currentRun.Records.Add(payrollRecord);


    }
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      await _payrollRunRepo.UpdateRunAsync(payrollRun);
    }
  }
}