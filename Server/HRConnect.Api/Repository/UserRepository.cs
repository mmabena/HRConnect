namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class UserRepository : IUserRepository
  {
    private readonly ApplicationDBContext _context;

    public UserRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<User> CreateUserAsync(User user)
    {
      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();

      return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
      var user = await _context.Users.FindAsync(id);
      if (user == null) return false;

      _context.Users.Remove(user);
      await _context.SaveChangesAsync();

      return true;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
      return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
      return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
      return !await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
      return await _context.Users.FindAsync(id);
    }

    public async Task<User?> UpdateUserAsync(int id, User user)
    {
      var existingUser = await _context.Users.FindAsync(id);
      if (existingUser == null) return null;

      _context.Entry(existingUser).CurrentValues.SetValues(user);
      await _context.SaveChangesAsync();

      return existingUser;
    }
  }
}