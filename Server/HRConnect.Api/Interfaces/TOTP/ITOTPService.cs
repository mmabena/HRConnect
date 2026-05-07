namespace HRConnect.Api.Interfaces.TOTP
{
  public interface ITOTPService
  {
    Task SendTotp(int userId);
    Task<string> GenerateCodeAsync(byte[] userSecret);
    Task<bool> ValidateCodeAsync(int userId, byte[] userSecret, string code);
    /// <summary>
    /// Consolidating Replay Store into the TOTPService even though it is part of 
    /// the algorithm to prevent replay attacks. Docs say this is left for the user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="stepCount">how long the each code remains valid and how
    /// often a new code is generated</param>
    Task<bool> IsReplayAsync(int userId, long stepCount);
    Task MarkUsedCodeAsync(int userId, long stepCount);
  }
}