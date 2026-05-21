namespace HRConnect.Api.Interfaces
{
  using System.Threading.Tasks;

  public interface IUserEmployeeHttpClient
  {
    Task<string> ResolveEmployeeFromUserIdAsync(int userId);
  }
}