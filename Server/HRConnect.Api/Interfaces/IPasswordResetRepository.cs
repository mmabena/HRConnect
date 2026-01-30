namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  public interface IPasswordResetRepository
  {
    Task<PasswordResetPin> CreatePinAsync(int userId, string email, string pin);
    Task<PasswordResetPin?> GetValidPinAsync(string email, string pin);
    Task MarkPinAsUsedAsync(int pinId);
    Task AddPasswordHistoryAsync(int userId, string passwordHash);
    Task<bool> IsPasswordUsedBefore(int userId, string passwordHash);
    Task ClearExpiredPinsAsync();
  }
}
