namespace HRConnect.Api.Services
{
  using System.Text;
  using System.Security.Claims;
  using System.IdentityModel.Tokens.Jwt;
  using HRConnect.Api.Interfaces;
  using System.Collections.Concurrent;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.IdentityModel.Tokens;
  public class AuthService : IAuthService
  {
    private sealed class LoginAttemptInfo
    {
      public int Attempts { get; set; }
      public DateTime? LockoutEnd { get; set; }
      public bool RequirePasswordReset { get; set; }
    }

    private static readonly ConcurrentDictionary<string, LoginAttemptInfo> _loginAttempts = new();

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _config;
    private readonly IPasswordResetRepository _passwordResetRepo;
    private readonly HRConnect.Api.Utils.IEmailService _emailService;

    public AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IConfiguration config, IPasswordResetRepository passwordResetRepo, HRConnect.Api.Utils.IEmailService emailService)
    {
      _userRepository = userRepository;
      _passwordHasher = passwordHasher;
      _config = config;
      _passwordResetRepo = passwordResetRepo;
      _emailService = emailService;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
      if (string.IsNullOrWhiteSpace(email) || !email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Email must be a @singular.co.za address.");
      }


      var user = await _userRepository.GetUserByEmailAsync(email);
      if (user == null)
        return null;

      var info = _loginAttempts.GetOrAdd(email.ToLowerInvariant(), _ => new LoginAttemptInfo());

      // If currently locked
      if (info.LockoutEnd.HasValue && info.LockoutEnd.Value > DateTime.UtcNow)
      {
        throw new ArgumentException("Account is locked. Try again later.");
      }

      // If lockout expired and user hasn't been prompted to reset yet
      if (info.LockoutEnd.HasValue && info.LockoutEnd.Value <= DateTime.UtcNow && info.Attempts >= 3 && !info.RequirePasswordReset)
      {
        info.RequirePasswordReset = true;
        throw new ArgumentException("Account locked. Please reset your password.");
      }

      var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
      if (result == PasswordVerificationResult.Failed)
      {
        // increment attempts
        info.Attempts++;
        // lock account when reaching 3 attempts
        if (info.Attempts >= 3)
        {
          info.LockoutEnd = DateTime.UtcNow.AddSeconds(60);
          info.RequirePasswordReset = false;
          throw new ArgumentException("Account locked for 60 seconds due to multiple failed login attempts.");
        }
        throw new ArgumentException("Invalid email or password.");
      }

      // successful login: clear attempt info
      _loginAttempts.TryRemove(email.ToLowerInvariant(), out _);

      return GenerateJwtToken(user);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
      return _user_repository_compat(email);
    }

    private Task<User?> _user_repository_compat(string email)
    {
      return _userRepository.GetUserByEmailAsync(email);
    }

    public async Task<(string pin, DateTime expiresAt)> ForgotPasswordAsync(string email)
    {
      if (string.IsNullOrWhiteSpace(email) || !email.EndsWith("@singular.co.za", System.StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Email must be a @singular.co.za address.");
      }

      var user = await _userRepository.GetUserByEmailAsync(email);
      if (user == null)
      {
        throw new KeyNotFoundException("User with this email not found.");
      }

      var pin = new Random().Next(1000, 9999).ToString(System.Globalization.CultureInfo.InvariantCulture);
      var resetPin = await _passwordResetRepo.CreatePinAsync(user.UserId, email, pin);

      var emailSubject = "Password Reset PIN";
      var emailBody = $"Your password reset PIN is: {pin}\n\nThis PIN is valid for 1 minute only.";
      await _emailService.SendEmailAsync(email, emailSubject, emailBody);

      return (pin, resetPin.ExpiresAt);
    }

    public async Task VerifyPinAsync(string email, string pin)
    {
      if (string.IsNullOrWhiteSpace(email) || !email.EndsWith("@singular.co.za", System.StringComparison.OrdinalIgnoreCase))
        throw new ArgumentException("Invalid email.");

      var resetPin = await _passwordResetRepo.GetValidPinAsync(email, pin);
      if (resetPin == null)
        throw new ArgumentException("Invalid or expired PIN.");
    }

    public async Task ResetPasswordAsync(HRConnect.Api.DTOs.User.ResetPasswordDto dto)
    {
      if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.EndsWith("@singular.co.za", System.StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Email must be a @singular.co.za address.");
      }

      if (string.IsNullOrWhiteSpace(dto.NewPassword) || !HRConnect.Api.Utils.PasswordValidator.IsValidPassword(dto.NewPassword))
      {
        throw new ArgumentException("Password does not meet complexity requirements. Minimum 8 chars, include uppercase, lowercase, digit and special character.");
      }

      var resetPin = await _passwordResetRepo.GetValidPinAsync(dto.Email, dto.Pin);
      if (resetPin == null)
        throw new ArgumentException("Invalid or expired PIN.");

      var user = await _userRepository.GetUserByEmailAsync(dto.Email);
      if (user == null)
        throw new KeyNotFoundException("User not found.");

      var newPasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
      if (await _passwordResetRepo.IsPasswordUsedBefore(user.UserId, newPasswordHash))
        throw new ArgumentException("You cannot reuse a previously used password.");

      await _passwordResetRepo.AddPasswordHistoryAsync(user.UserId, user.PasswordHash);

      user.PasswordHash = newPasswordHash;
      await _userRepository.UpdateUserAsync(user.UserId, user);

      await _passwordResetRepo.MarkPinAsUsedAsync(resetPin.Id);
      // clear any login attempt state for this user after successful password reset
      _loginAttempts.TryRemove(dto.Email.ToLowerInvariant(), out _);
    }

    private string GenerateJwtToken(User user)
    {
      var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("role", user.Role.ToString())
        };

      /// <summary>
      /// Reads the JWT signing secret from configuration and creates signing credentials.
      /// Supports both base64-encoded secrets and plain text secrets.
      /// </summary>
      var secretValue = _config["JwtSettings:Secret"] ?? string.Empty;
      byte[] keyBytes;
      try
      {
        keyBytes = Convert.FromBase64String(secretValue);
      }
      catch (FormatException)
      {
        keyBytes = Encoding.UTF8.GetBytes(secretValue);
      }

      var securityKey = new SymmetricSecurityKey(keyBytes);
      var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      /// <summary>
      /// Reads the JWT expiry duration from configuration and creates a signed JWT token.
      /// If the value is missing or invalid, a default of 60 minutes is used.
      /// </summary>
      var expiryStr = _config["JwtSettings:ExpiryMinutes"] ?? "60";
      if (!int.TryParse(expiryStr, out var expiryMinutes)) expiryMinutes = 60;

      var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }

}