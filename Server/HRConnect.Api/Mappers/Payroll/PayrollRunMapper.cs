namespace HRConnect.Api.Mappers.Payroll
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Models;
  public static class PayrollRunMapper
  {
    public static PayrollRunDto ToPayrollRunDto(this PayrollRun runModel)
    {
      return new PayrollRunDto { };
    }
    public static PayrollRun ToPayrollRunFromDto(this PayrollRunDto dto)
    {
      return new PayrollRun { };
    }
  }
}