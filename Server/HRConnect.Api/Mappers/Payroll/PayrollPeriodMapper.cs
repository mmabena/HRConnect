namespace HRConnect.Api.Mappers.Payroll
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;

  public static class PayrollPeriodMapper
  {
    public static PayrollPeriodDto ToPayrollPeriodDto(this PayrollPeriod periodModel)
    {
      return new PayrollPeriodDto { };
    }
    public static PayrollPeriod ToPayrollPeriodFromDto(this PayrollPeriodDto dto)
    {
      return new PayrollPeriod { };
    }
  }
}

