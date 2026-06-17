namespace HRConnect.Api.Interfaces.TOTP
{
  public interface ITOTPRepository
  {
    Task<bool> IsReplay(int userId, long stepCount);
    Task MarkUsedAsync(int userId, long stepCount);
    Task Save();
  }
}