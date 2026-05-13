namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Data;

  public class TOTPRepository : ITOTPRepository
  {
    private readonly ApplicationDBContext _context;
    public TOTPRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task<bool> IsReplay(int userId, long stepCount)
    {
      var state = await _context.TOTPStates.FindAsync(userId);

      if (state == null) return false;

      if (stepCount <= state.LastUsedTimeStamp) return true;
      //Default to code being replayed
      return false;
    }
    public async Task Save()
    {
      _ = await _context.SaveChangesAsync();
    }

    public async Task MarkUsedAsync(int userId, long stepCount)
    {
      var state = await _context.TOTPStates.FindAsync(userId);
      if (state == null)
      {
        state = new TOTPState
        {
          UserId = userId,
          LastUsedTimeStamp = stepCount
        };
        _ = await _context.TOTPStates.AddAsync(state);
      }
      else
      {
        state.LastUsedTimeStamp = stepCount;
      }
      _ = await _context.SaveChangesAsync();
    }
  }
}