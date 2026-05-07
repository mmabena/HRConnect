namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Security.Cryptography;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.Utils;
  using Microsoft.EntityFrameworkCore;

  public class UserService : IUserService
  {
    private readonly ApplicationDBContext _context;
    private readonly IUserRepository _userRepo;
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _passwordHasher;
    private static readonly char[] UppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] LowercaseChars = "abcdefghijkmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] DigitChars = "23456789".ToCharArray();
    private static readonly char[] SpecialChars = "!@#$%^&*".ToCharArray();
    private static readonly char[] AllPasswordChars = UppercaseChars
      .Concat(LowercaseChars)
      .Concat(DigitChars)
      .Concat(SpecialChars)
      .ToArray();

    public UserService(
      ApplicationDBContext context,
      IUserRepository userRepo,
      Microsoft.AspNetCore.Identity.IPasswordHasher<User> passwordHasher)
    {
      _context = context;
      _userRepo = userRepo;
      _passwordHasher = passwordHasher;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
      await SyncEmployeeUsersAsync();
      return await _userRepo.GetAllUsersAsync();
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

    public async Task<List<UserRoleOptionDto>> GetRoleOptionsAsync()
    {
      return await Task.FromResult(
        Enum.GetValues<UserRole>()
          .Select(role => new UserRoleOptionDto
          {
            RoleId = (int)role,
            Name = role.ToString()
          })
          .ToList());
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

    public async Task<User?> UpdateUserRoleAsync(int id, UpdateUserRoleRequestDto dto)
    {
      if (!Enum.IsDefined(typeof(UserRole), dto.RoleId))
      {
        throw new ArgumentException("Invalid role id.");
      }

      var existing = await _userRepo.GetUserByIdAsync(id);
      if (existing == null)
      {
        return null;
      }

      existing.Role = (UserRole)dto.RoleId;
      return await _userRepo.UpdateUserAsync(id, existing);
    }

    public async Task<User?> UpdateEmployeeUserRoleAsync(string employeeId, UpdateUserRoleRequestDto dto)
    {
      if (string.IsNullOrWhiteSpace(employeeId))
      {
        throw new ArgumentException("Employee id is required.");
      }

      if (!Enum.IsDefined(typeof(UserRole), dto.RoleId))
      {
        throw new ArgumentException("Invalid role id.");
      }

      var employee = await _context.Employees
        .AsNoTracking()
        .FirstOrDefaultAsync(existingEmployee => existingEmployee.EmployeeId == employeeId);

      if (employee == null)
      {
        return null;
      }

      if (string.IsNullOrWhiteSpace(employee.Email))
      {
        throw new ArgumentException("Employee does not have an email address.");
      }

      var user = await EnsureUserForEmailAsync(employee.Email);
      user.Role = (UserRole)dto.RoleId;

      await _context.SaveChangesAsync();
      return user;
    }

    public async Task SyncEmployeeUsersAsync()
    {
      var employees = await _context.Employees
        .AsNoTracking()
        .Where(employee => !string.IsNullOrWhiteSpace(employee.Email))
        .Select(employee => employee.Email.Trim())
        .Distinct()
        .ToListAsync();

      if (employees.Count == 0)
      {
        return;
      }

      var existingUserEmails = await _context.Users
        .Select(user => user.Email)
        .ToListAsync();

      var existingUserEmailSet = existingUserEmails
        .Where(email => !string.IsNullOrWhiteSpace(email))
        .Select(email => email.Trim())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

      var missingUsers = employees
        .Where(email => !existingUserEmailSet.Contains(email))
        .Select(CreateNormalUser)
        .ToList();

      if (missingUsers.Count == 0)
      {
        return;
      }

      await _context.Users.AddRangeAsync(missingUsers);
      await _context.SaveChangesAsync();
    }

    public Task<bool> DeleteUserAsync(int id)
    {
      return _userRepo.DeleteUserAsync(id);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(string email)
    {
      if (string.IsNullOrWhiteSpace(email))
        return null;

      var user = await _userRepo.GetUserByEmailAsync(email);
      
      if (user == null) 
      return null;

      var employee = await _userRepo.GetEmployeeByEmailAsync(email);

      return new CurrentUserDto
      {
        UserId = user.UserId,
        Email = user.Email,
        Role = user.Role.ToString(),
        FullName = employee != null
          ? $"{employee.Name} {employee.Surname}"
          : null,
        JobTitle = employee?.Position?.PositionTitle
      };
    }
    public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto dto)
    {
      var user = await _userRepo.GetUserByEmailAsync(dto.Email);
      if (user == null)
        throw new ArgumentException("User not found.");

      // Verify current password
      var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
      if (verificationResult == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        throw new ArgumentException("Current password is incorrect.");

      if (!PasswordValidator.IsValidPassword(dto.NewPassword))
        throw new ArgumentException("New password does not meet complexity requirements. Minimum 8 chars, include uppercase, lowercase, digit and special character.");

      user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
      await _userRepo.UpdateUserAsync(user.UserId, user);
      return true;
    }

    private User CreateNormalUser(string email)
    {
      var user = new User
      {
        Email = email.Trim(),
        Role = UserRole.NormalUser,
        CreatedAt = DateTime.UtcNow,
      };

      user.PasswordHash = _passwordHasher.HashPassword(user, GenerateTemporaryPassword());
      return user;
    }

    private async Task<User> EnsureUserForEmailAsync(string email)
    {
      var normalizedEmail = email.Trim();

      var existingUser = await _context.Users
        .FirstOrDefaultAsync(user => user.Email == normalizedEmail);

      if (existingUser != null)
      {
        return existingUser;
      }

      var newUser = CreateNormalUser(normalizedEmail);
      await _context.Users.AddAsync(newUser);
      await _context.SaveChangesAsync();

      return newUser;
    }

    private static string GenerateTemporaryPassword()
    {
      var passwordChars = new List<char>
      {
        GetRandomCharacter(UppercaseChars),
        GetRandomCharacter(LowercaseChars),
        GetRandomCharacter(DigitChars),
        GetRandomCharacter(SpecialChars),
      };

      while (passwordChars.Count < 12)
      {
        passwordChars.Add(GetRandomCharacter(AllPasswordChars));
      }

      for (var i = passwordChars.Count - 1; i > 0; i--)
      {
        var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
        (passwordChars[i], passwordChars[swapIndex]) = (passwordChars[swapIndex], passwordChars[i]);
      }

      return new string(passwordChars.ToArray());
    }

    private static char GetRandomCharacter(char[] alphabet)
    {
      return alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
    }
  }
}
