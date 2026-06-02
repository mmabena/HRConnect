namespace HRConnect.Api.Interfaces
{
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.User;

  public interface IUserHttpClient
  {
    Task<UserRegisterDto> ResolveUserFromId(int userId);
  }
}