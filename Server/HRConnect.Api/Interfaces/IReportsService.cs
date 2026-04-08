namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models.Payroll;
  public interface IReportsService
  {
    Task WriteExcelAsync(PayrollRun run);
  }
}