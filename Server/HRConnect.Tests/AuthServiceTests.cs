namespace HRConnect.Tests.Services
{
  using Moq;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Configuration;
  public class AuthServiceTests
  {
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IPasswordResetRepository> _passwordResetRepoMock;
    private readonly Mock<HRConnect.Api.Utils.IEmailService> _emailServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
      _userRepoMock = new Mock<IUserRepository>();
      _passwordHasherMock = new Mock<IPasswordHasher<User>>();
      _configMock = new Mock<IConfiguration>();
      _passwordResetRepoMock = new Mock<IPasswordResetRepository>();
      _emailServiceMock = new Mock<HRConnect.Api.Utils.IEmailService>();

      // Mock JWT configuration
      _configMock.Setup(c => c["JwtSettings:Secret"])
                .Returns("my-super-secret-key-that-is-long-enough-for-jwt-tokens");
      _configMock.Setup(c => c["JwtSettings:Issuer"])
                .Returns("HRConnect");
      _configMock.Setup(c => c["JwtSettings:Audience"])
                .Returns("HRConnectUsers");
      _configMock.Setup(c => c["JwtSettings:ExpiryMinutes"])
                .Returns("60");

      _authService = new AuthService(
          _userRepoMock.Object,
          _passwordHasherMock.Object,
          _configMock.Object,
          _passwordResetRepoMock.Object,
          _emailServiceMock.Object
      );
    }

    [Fact]
    public async Task LoginAsyncValidSuperUserReturnsToken()
    {
      // Arrange
      var email = "superuser@singular.co.za";
      var password = "password123";
      var user = new User
      {
        Email = email,
        PasswordHash = "hashedPassword",
        Role = UserRole.SuperUser
      };

      _userRepoMock.Setup(r => r.GetUserByEmailAsync(email))
                   .ReturnsAsync(user);

      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, password))
                         .Returns(PasswordVerificationResult.Success);

      // Act
      var token = await _authService.LoginAsync(email, password);

      // Assert
      Assert.NotNull(token);
    }

    [Fact]
    public async Task LoginAsyncValidNormalUserReturnsToken()
    {
      // Arrange
      var email = "normaluser@singular.co.za";
      var password = "password";
      var user = new User
      {
        Email = email,
        PasswordHash = "hashedPassword",
        Role = UserRole.NormalUser
      };

      _userRepoMock.Setup(r => r.GetUserByEmailAsync(email))
                   .ReturnsAsync(user);

      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, password))
                         .Returns(PasswordVerificationResult.Success);

      // Act
      var token = await _authService.LoginAsync(email, password);

      // Assert
      Assert.NotNull(token);
    }

    [Fact]
    public async Task LoginAsyncInvalidPasswordThrowsArgumentException()
    {
      // Arrange
      var email = "user@singular.co.za";
      var password = "wrongpassword";
      var user = new User { Email = email, PasswordHash = "hashedPassword", Role = UserRole.NormalUser };

      _userRepoMock.Setup(r => r.GetUserByEmailAsync(email))
                   .ReturnsAsync(user);

      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, password))
                         .Returns(PasswordVerificationResult.Failed);

      // Act & Assert
      await Assert.ThrowsAsync<ArgumentException>(() => _authService.LoginAsync(email, password));
    }

    [Fact]
    public async Task LoginAsyncUserNotFoundReturnsNull()
    {
      // Arrange
      _userRepoMock.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                   .ReturnsAsync((User)null!);

      // Act
      var token = await _authService.LoginAsync("nonexistent@singular.co.za", "anyPassword");

      // Assert
      Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsyncInvalidEmailDomainReturnsNull()
    {
      // Arrange
      var email = "user@example.com";
      var password = "password123";

      // Act
      var token = await _authService.LoginAsync(email, password);

      // Assert
      Assert.Null(token);
    }

    [Fact]
    public async Task LoginAsyncThreeFailedAttemptsLocksAccount()
    {
      // Arrange
      var email = "user@singular.co.za";
      var password = "wrongpassword";
      var user = new User { Email = email, PasswordHash = "hashedPassword", Role = UserRole.NormalUser };

      _userRepoMock.Setup(r => r.GetUserByEmailAsync(email))
                   .ReturnsAsync(user);

      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, password))
                         .Returns(PasswordVerificationResult.Failed);

      // Act & Assert - first two attempts throw exception with generic message
      await Assert.ThrowsAsync<ArgumentException>(() => _authService.LoginAsync(email, password));
      await Assert.ThrowsAsync<ArgumentException>(() => _authService.LoginAsync(email, password));

      // third attempt should lock account
      var ex = await Assert.ThrowsAsync<ArgumentException>(() => _authService.LoginAsync(email, password));
      Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsyncDuringLockoutThrowsLockoutMessage()
    {
      // Arrange
      var email = "user@singular.co.za";
      var password = "wrongpassword";
      var user = new User { Email = email, PasswordHash = "hashedPassword", Role = UserRole.NormalUser };

      _userRepoMock.Setup(r => r.GetUserByEmailAsync(email))
                   .ReturnsAsync(user);

      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, password))
                         .Returns(PasswordVerificationResult.Failed);

      // Lock account with 3 failed attempts
      for (int i = 0; i < 3; i++)
      {
        try { await _authService.LoginAsync(email, password); }
        catch { /* expected */ }
      }

      // Act & Assert - subsequent attempt during lockout
      var ex = await Assert.ThrowsAsync<ArgumentException>(() => _authService.LoginAsync(email, password));
      Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsyncSuccessfulLoginClearsAttempts()
    {
      // Arrange
      var email = "user@singular.co.za";
      var password = "password123";
      var user = new User { Email = email, PasswordHash = "hashedPassword", Role = UserRole.NormalUser };

      _userRepoMock.Setup(r => r.GetUserByEmailAsync(email))
                   .ReturnsAsync(user);

      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, It.IsAny<string>()))
                         .Returns(PasswordVerificationResult.Failed);

      // First failed attempt
      try { await _authService.LoginAsync(email, "wrongpassword"); }
      catch { /* expected */ }

      // Now successful login
      _passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, user.PasswordHash, password))
                         .Returns(PasswordVerificationResult.Success);

      var token = await _authService.LoginAsync(email, password);

      // Assert - token returned, attempts cleared
      Assert.NotNull(token);
    }
  }
}
