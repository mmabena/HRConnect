namespace HRConnect.Api.Interfaces.TOTP
{
  using HRConnect.Api.Models;

  public interface IMFAUserSecretsRepository
  {
    Task AddUserSecretAsync(MFAUserSecret secret);
    Task<MFAUserSecret?> GetUserSecretAsync(int userId);
  }
}