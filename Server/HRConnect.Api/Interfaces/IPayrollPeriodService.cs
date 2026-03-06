namespace HRConnect.Api.Interfaces
{
  using System.Threading.Tasks;

  public interface IPayrollPeriodService
  {
    Task ExecuteRolloverAsync();
  }
}