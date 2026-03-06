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

    }
    public async Task<PayrollRun?> GetRunByDateAsync(DateTime dateTime)
    {

    }
    public async Task<PayrollRun> GetCurrentRunAsync()
    {
      var payrun = await _payrollRunRepo.GetCurrentRunAsync();

      if (payrun == null)
        return null;

    }
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {

    }
  }
}