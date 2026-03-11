namespace HRConnect.Api.Mappers.Payroll
{
  using DTOs.Payroll;
  using Models.Payroll;

  public static class PayrollPeriodMapper
  {
    public static PayrollPeriodDto ToPayrollPeriodDto(this PayrollPeriod periodModel)
    {
      return new PayrollPeriodDto
      {
        PayrollPeriodId = periodModel.PayrollPeriodId,
        StartDate = periodModel.StartDate,
        EndDate = periodModel.EndDate,
        IsClosed = periodModel.IsClosed,
        IsLocked = periodModel.IsLocked,
        Runs = periodModel.Runs
      };
    }
    public static PayrollPeriod ToPayrollPeriodFromDto(this PayrollPeriodDto dto)
    {
      return new PayrollPeriod
      {
        PayrollPeriodId = dto.PayrollPeriodId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        IsClosed = dto.IsClosed,
        IsLocked = dto.IsLocked,
        Runs = dto.Runs
      };
    }
  }
}

