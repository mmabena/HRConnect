namespace HRConnect.Api.Mappers.Payroll.Earnings
{
  using HRConnect.Api.DTOs.Payroll.Earnings;
  using HRConnect.Api.Models.Payroll.Earnings;

  public static class PayrollEarningMapper
  {
    public static PayrollEarningDto ToPayrollEarningDto(this PayrollEarning payrollEarning)
    {
      return new PayrollEarningDto
      {
        PayrollEarningId = payrollEarning.PayrollEarningId,
        ShortDescription = payrollEarning.ShortDescription,
        LongDescription = payrollEarning.LongDescription,
        Taxable = payrollEarning.Taxable,
        TaxCode = payrollEarning.TaxCode,
        CanProRata = payrollEarning.CanProRata,
        IsActive = payrollEarning.IsActive
      };
    }

    public static PayrollEarning ToPayrollEarningModel(this PayrollEarningAddDto payrollEarningAddDto)
    {
      return new PayrollEarning
      {
        PayrollEarningId = "",
        ShortDescription = payrollEarningAddDto.ShortDescription,
        LongDescription = payrollEarningAddDto.LongDescription,
        Taxable = payrollEarningAddDto.Taxable,
        TaxCode = payrollEarningAddDto.TaxCode,
        HourlyRate = payrollEarningAddDto.HourlyRate,
        CanProRata = payrollEarningAddDto.CanProRata,
        IsActive = payrollEarningAddDto.IsActive
      };
    }
  }
}
