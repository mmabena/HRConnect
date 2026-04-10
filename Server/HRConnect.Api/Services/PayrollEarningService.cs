namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Payroll.Earnings;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Payroll.Earnings;
  using HRConnect.Api.Mappers.Payroll.Earnings;
  using HRConnect.Api.Models.Payroll.Earnings;

  public class PayrollEarningService(IPayrollEarningRepository payrollEarningRepository) : IPayrollEarningService
  {
    private readonly IPayrollEarningRepository _payrollEarningRepository = payrollEarningRepository;
    public Task<PayrollEarningDto> AddPayrollEarningAsync(PayrollEarningAddDto payrollEarningAddDto)
    {
      PayrollEarning newPayrollEarning = payrollEarningAddDto.ToPayrollEarningModel();
      if (string.IsNullOrEmpty(newPayrollEarning.PayrollEarningId))
      {

      }
      else
      {
        throw new ArgumentException("Pay roll earning codes are auto generated and cannot be user input");
      }
    }

    public Task<List<PayrollEarningDto>> GetAllPayrollEarningsAsync()
    {
      throw new NotImplementedException();
    }

    public Task<PayrollEarningDto> GetPayrollEarningByIdAsync(string payrollEarningId)
    {
      throw new NotImplementedException();
    }

    public Task<List<PayrollEarningDto>> GetPayrollEarningByTaxCode(int taxCode)
    {
      throw new NotImplementedException();
    }

    public Task<PayrollEarningDto> UpdatePayrollEarningAsync(PayrollEarningUpdateDto payrollEarningUpdateDto)
    {
      throw new NotImplementedException();
    }
  }
}
