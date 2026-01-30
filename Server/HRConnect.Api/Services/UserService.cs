namespace HRConnect.Api.Services
{
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Mappers;

  public class UserService : IUserService
  {
    private readonly IUserRepository _userRepo;
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _passwordHasher;

    public UserService(IUserRepository userRepo, Microsoft.AspNetCore.Identity.IPasswordHasher<User> passwordHasher)
    {
      _userRepo = userRepo;
      _passwordHasher = passwordHasher;
    }

    public Task<List<User>> GetAllUsersAsync()
    {
      return _userRepo.GetAllUsersAsync();
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
      return _userRepo.GetUserByIdAsync(id);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
      return _userRepo.GetUserByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(CreateUserRequestDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.EndsWith("@singular.co.za", System.StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Email must be a @singular.co.za address.");
      }

      if (string.IsNullOrWhiteSpace(dto.Password) || !PasswordValidator.IsValidPassword(dto.Password))
      {
        throw new ArgumentException("Password does not meet complexity requirements. Minimum 8 chars, include uppercase, lowercase, digit and special character.");
      }

      var isUnique = await _userRepo.IsEmailUniqueAsync(dto.Email);
      if (!isUnique)
      {
        throw new ArgumentException("Email already exists.");
      }

      var user = dto.ToUserFromCreateUserRequestDto();
      user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

      return await _userRepo.CreateUserAsync(user);
    }

    public async Task<User?> UpdateUserAsync(int id, UpdateUserRequestDto dto)
    {
      var existing = await _userRepo.GetUserByIdAsync(id);
      if (existing == null) return null;

      if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.EndsWith("@singular.co.za", System.StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Email must be a @singular.co.za address.");
      }

      existing.Email = dto.Email;
      existing.Role = dto.Role;

      if (!string.IsNullOrWhiteSpace(dto.Password))
      {
        if (!PasswordValidator.IsValidPassword(dto.Password))
        {
          throw new ArgumentException("Password does not meet complexity requirements. Minimum 8 chars, include uppercase, lowercase, digit and special character.");
        }

        existing.PasswordHash = _passwordHasher.HashPassword(existing, dto.Password);
      }

      return await _userRepo.UpdateUserAsync(id, existing);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
      return _userRepo.DeleteUserAsync(id);
    }
  }
}
