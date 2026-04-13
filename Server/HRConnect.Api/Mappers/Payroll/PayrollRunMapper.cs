namespace HRConnect.Api.Mappers.Payroll
{
  using HRConnect.Api.DTOs.Payroll;
  using HRConnect.Api.Models.Payroll;
  public static class PayrollRunMapper
  {
    public static PayrollRunDto ToPayrollRunDto(this PayrollRun runModel)
    {
      return new PayrollRunDto
      {
        PeriodId = runModel.PeriodId,
        // Period = runModel.Period,
        PeriodDate = runModel.PeriodDate,
        IsFinalised = runModel.IsFinalised,
        FinalisedDate = runModel.FinalisedDate,
        PayrollRunNumber = runModel.PayrollRunNumber,
        Records = runModel.Records
      };
    }

  }
}