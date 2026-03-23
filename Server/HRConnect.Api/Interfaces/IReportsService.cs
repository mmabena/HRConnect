namespace HRConnect.Api.Interfaces
{
  public interface IReportsService
  {
    Task WriteExcelAsync(PayrollRun run, string rootPath);
  }
}