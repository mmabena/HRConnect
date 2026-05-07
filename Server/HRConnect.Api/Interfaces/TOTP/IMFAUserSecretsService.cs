namespace HRConnect.Api.Interfaces.TOTP
{
  public interface IMFAUserSecretsService
  {
    Task<byte[]> GetOrCreateUserSecretAsync(int userId);
  }
}