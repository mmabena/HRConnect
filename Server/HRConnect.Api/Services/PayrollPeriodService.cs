namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Utils.Payroll;
  using HRConnect.Api.Mappers.Payroll;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.DTOs.Payroll;

  public class PayrollPeriodService : IPayrollPeriodService
  {
    private readonly IPayrollPeriodRepository _payrollPeriodRepository;
    public PayrollPeriodService(IPayrollPeriodRepository payrollPeriodRepository)
    {
      _payrollPeriodRepository = payrollPeriodRepository;
    }
    public async Task<IEnumerable<PayrollPeriodDto>> GetAllPeriodsAsync()
    {
      var periods = await _payrollPeriodRepository.GetAllPayrollPeriod();
      return periods.Select(p => p.ToPayrollPeriodDto()).ToList();
    }
    public async Task<PayrollPeriodDto?> GetPayrollPeriodByGuidAsync(int id)
    {
      var period = await _payrollPeriodRepository.GetByIdAsync(id);
      return period;
    }
    public async Task<PayrollPeriod?> GetActivePeriod(DateTime dateTime)
    {
      var period = await _payrollPeriodRepository.GetActivePeriod(dateTime);
      if (period == null)
        return null;
      return period;
    }
    public async Task<PayrollPeriodDto> CreatePeriodAsync(PayrollPeriod payrollPeriod)
    {
      //Get the current payroll period
      (DateTime start, DateTime end) = PayrollUtil.GetCurrectFinancialPeriod();
      // payrollPeriod.PayrollPeriodId = ();
      payrollPeriod.StartDate = start;
      payrollPeriod.EndDate = end;
      payrollPeriod.IsLocked = false;
      payrollPeriod.IsClosed = false;
      payrollPeriod.Runs = new List<PayrollRun>();
      var periodDto = await _payrollPeriodRepository.CreatePeriodAsync(payrollPeriod);
      return periodDto;
    }

    public async Task UpdateAsync(PayrollPeriod payrollPeriod)
    {
      await _payrollPeriodRepository.UpdateAsync(payrollPeriod);
    }
    public async Task<PayrollPeriod?> GetLastPeriodAsync()
    {
      return await _payrollPeriodRepository.GetLastPeriodAsync();
    }
    public async Task<PayrollPeriod?> GetCurrentActivePayrollPeriod()
    {
      // The repository does not have GetCurrentActivePayrollPeriod.
      // Use an existing method, e.g., GetActivePeriod, with DateTime.Now.
      //return await _payrollPeriodRepository.GetActivePeriod(DateTime.Now);
      throw new NotImplementedException("GetCurrentActivePayrollPeriod is not implemented in the repository. Consider using GetActivePeriod with DateTime.Now instead.");
    }
  }
}