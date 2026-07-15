namespace HRConnect.Api.Interfaces.TOTP
{
  public interface ITOTPService
  {
    ///<summary>
    /// Method has mulitple related responsibilities for sending Time-Base
    /// One-Time-Pin. <see cref="MFAUserSecretsService.GetOrCreateUserSecretAsync(int)"
    ///is used to create user secret of which the pin is based off of.. 
    ///</summary>
    ///<remarks> <a href="datatracker.ietf.org/doc/html/rfc6238">
    /// See RFC6238 for algorithm details and recommended implementations
    /// </a>
    /// </remarks>
    Task SendTotpAndNotify(int userId);
     /// <summary>
    /// This method confirms that role update made by <see
    /// cref="IUserService.UpdateUserRoleAsync(int, DTOs.User.UpdateUserRoleRequestDto)" 
    /// is carried out and role update is finalised throughout the system
    /// </summary>
    /// <param name="userId">User whom the role is being updated</param>   string GenerateCode(byte[] userSecret);
    Task ConfirmUserRoleUpdateAsync(int userId);
    Task<bool> ValidateCodeAsync(int userId, byte[] userSecret, string code);
    /// <summary>
    /// Consolidating Replay Store into the TOTPService even though it is part of 
    /// the algorithm to prevent replay attacks. Docs say this is left for the user
    /// </summary>
    /// <param name="userId">User for which the code is being verified for</param>
    /// <param name="stepCount">how long  code remains valid and how
    /// often a new code is generated</param>
    Task<bool> IsReplayAsync(int userId, long stepCount);
    Task MarkUsedCodeAsync(int userId, long stepCount);
  }
}