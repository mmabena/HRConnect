namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Models;
  using OtpNet;

/// <summary>
/// This class talks to the TOTPService to get or generate a user secret
/// of which the OTP can be derived and verified.
/// </summary>
  public class MFAUserSecretsService : IMFAUserSecretsService
  {
    private readonly IMFAUserSecretsRepository _userSecretRepo;
    public MFAUserSecretsService(IMFAUserSecretsRepository userSecretRepo)
    {
      _userSecretRepo = userSecretRepo;
    }

    public async Task<byte[]> GetOrCreateUserSecretAsync(int userId)
    {
      MFAUserSecret? state = await _userSecretRepo.GetUserSecretAsync(userId);
      if (state != null)
        return state.EncryptedUserSecret;

      byte[] secret = KeyGeneration.GenerateRandomKey(OtpHashMode.Sha256);

      await _userSecretRepo.AddUserSecretAsync(new MFAUserSecret
      {
        UserId = userId,
        EncryptedUserSecret = secret,
        CreatedAt = DateTime.Now,
        KeyVersion = 1
      });

      return secret;
    }
  }
}