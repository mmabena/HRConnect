namespace HRConnect.Api.Mappers.Payroll
{
  using DTOs.Payroll;
  using Models.Payroll;

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