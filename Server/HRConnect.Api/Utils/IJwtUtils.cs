namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Models;

  public interface IJwtUtils
  {
    string GenerateToken(User user);
  }
}
