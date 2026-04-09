namespace HRConnect.Api.Interfaces
{

  public interface IPensionOptionRepository
  {
    Task<decimal> GetPensionOptionPercentageByIdAsync(int id);
  }
}
