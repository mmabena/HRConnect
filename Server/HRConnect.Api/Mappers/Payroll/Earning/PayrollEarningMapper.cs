namespace HRConnect.Api.Mappers.Payroll.Earning
{
  using HRConnect.Api.DTOs.Payroll.Earning;
  using HRConnect.Api.Models.Payroll.Earning;

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
        TaxPercentage = payrollEarning.TaxPercentage,
        CanProRata = payrollEarning.CanProRata,
        IsOnGoing = payrollEarning.IsOnGoing,
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
        TaxPercentage = payrollEarningAddDto.TaxPercentage,
        OvertimeHourMultiplier = payrollEarningAddDto.OvertimeHourMultiplier,
        CanProRata = payrollEarningAddDto.CanProRata,
        IsOnGoing = payrollEarningAddDto.IsOnGoing,
        IsActive = true
      };
    }
  }
}
