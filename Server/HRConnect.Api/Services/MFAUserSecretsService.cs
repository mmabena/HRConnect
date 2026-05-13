namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Models;
  using OtpNet;

  public class MFAUserSecretsService : IMFAUserSecretsService
  {
    private readonly ISecretsProtector _protector;
    private readonly IMFAUserSecretsRepository _userSecretRepo;
    public MFAUserSecretsService(IMFAUserSecretsRepository userSecretRepo,
    ISecretsProtector protector)
    {
      _userSecretRepo = userSecretRepo;
      _protector = protector;
    }

    public async Task<byte[]> GetOrCreateUserSecretAsync(int userId)
    {
      MFAUserSecret? state = await _userSecretRepo.GetUserSecretAsync(userId);
      if (state != null)
        return _protector.UnWrap(state.EncryptedUserSecret);

      //Create a new Key if none exists
      byte[] secret = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha256);

      await _userSecretRepo.AddUserSecretAsync(new MFAUserSecret
      {
        UserId = userId,
        EncryptedUserSecret = _protector.Wrap(secret),
        CreatedAt = DateTime.Now,
        KeyVersion = 1
      });

      return secret;
    }
  }
}