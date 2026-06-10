namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Data;

  public class MFAUserSecretsRepository : IMFAUserSecretsRepository
  {
    private readonly ApplicationDBContext _context;
    public MFAUserSecretsRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task AddUserSecretAsync(MFAUserSecret secret)
    {
      await _context.UserSecrets.AddAsync(secret);
      await _context.SaveChangesAsync();
    }
    public async Task<MFAUserSecret?> GetUserSecretAsync(int userId)
    {
      return await _context.UserSecrets.FindAsync(userId);
    }
  }
}