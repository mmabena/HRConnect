namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  public class PasswordResetRepository : IPasswordResetRepository
  {
    private readonly ApplicationDBContext _context;

    public PasswordResetRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<PasswordResetPin> CreatePinAsync(int userId, string email, string pin)
    {
      var resetPin = new PasswordResetPin
      {
        UserId = userId,
        Email = email,
        Pin = pin,
        ExpiresAt = DateTime.UtcNow.AddMinutes(1),
        IsUsed = false,
        CreatedAt = DateTime.UtcNow
      };

      await _context.PasswordResetPins.AddAsync(resetPin);
      await _context.SaveChangesAsync();

      return resetPin;
    }

    public async Task<PasswordResetPin?> GetValidPinAsync(string email, string pin)
    {
      return await _context.PasswordResetPins
          .FirstOrDefaultAsync(p =>
              p.Email == email &&
              p.Pin == pin &&
              !p.IsUsed &&
              p.ExpiresAt > DateTime.UtcNow);
    }

    public async Task MarkPinAsUsedAsync(int pinId)
    {
      var resetPin = await _context.PasswordResetPins.FindAsync(pinId);
      if (resetPin != null)
      {
        resetPin.IsUsed = true;
        _context.PasswordResetPins.Update(resetPin);
        await _context.SaveChangesAsync();
      }
    }

    public async Task AddPasswordHistoryAsync(int userId, string passwordHash)
    {
      var history = new PasswordHistory
      {
        UserId = userId,
        PasswordHash = passwordHash,
        ChangedAt = DateTime.UtcNow
      };

      await _context.PasswordHistories.AddAsync(history);
      await _context.SaveChangesAsync();
    }

    public async Task<bool> IsPasswordUsedBefore(int userId, string passwordHash)
    {
      /// <summary>
      /// Checks whether a given password hash has been used previously by a specific user.
      /// </summary>
      return await _context.PasswordHistories
          .AnyAsync(p => p.UserId == userId && p.PasswordHash == passwordHash);
    }

    public async Task ClearExpiredPinsAsync()
    {
      var expiredPins = await _context.PasswordResetPins
          .Where(p => p.ExpiresAt < DateTime.UtcNow && !p.IsUsed)
          .ToListAsync();

      if (expiredPins.Count > 0)
      {
        _context.PasswordResetPins.RemoveRange(expiredPins);
        await _context.SaveChangesAsync();
      }
    }
  }
}
