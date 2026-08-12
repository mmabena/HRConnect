namespace HRConnect.Tests
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Services;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.User;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.DataProtection;
  using Moq;

  public class UserServiceTest
  {
    private readonly Mock<ITOTPService> _otpServiceMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly ApplicationDBContext _context;

    private static readonly char[] UpperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] LowerCaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] DigitChars = "1234567890".ToCharArray();
    private static readonly char[] SpecialChars = "!@#$%^&*".ToCharArray();
    private static readonly char[] AllPossibleChars = UpperCaseChars
      .Concat(LowerCaseChars)
      .Concat(DigitChars)
      .Concat(SpecialChars)
      .ToArray();

    public UserServiceTest()
    {
      _otpServiceMock = new Mock<ITOTPService>();
      _userRepoMock = new Mock<IUserRepository>();
      _employeeRepoMock = new Mock<IEmployeeRepository>();
      _passwordHasherMock = new Mock<IPasswordHasher<User>>();
    }
    private static ApplicationDBContext GetDbContext()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
   .UseInMemoryDatabase(Guid.NewGuid().ToString())
   .Options;
      // Create a mock IDataProtectionProvider
      var mockProvider = new Mock<IDataProtectionProvider>();
      // Setup CreateProtector to return a dummy protector
      var mockProtector = new Mock<IDataProtector>();
      mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(mockProtector.Object);
      return new ApplicationDBContext(options, mockProtector.Object);
    }

    [Fact]
    public async Task GetRoleOptionsAsyncShouldReturnRoleOptions()
    {
      UserService userService = new UserService(GetDbContext(), _otpServiceMock.Object, _userRepoMock.Object,
          _passwordHasherMock.Object, _employeeRepoMock.Object);


      var result = await userService.GetRoleOptionsAsync();
      var expected = Enum.GetValues<UserRole>()
          .Select(role => new UserRoleOptionDto
          {
            RoleId = (int)role,
            Name = role.ToString()
          }).ToList();

      //Should return the same amount of roles
      Assert.Equal(expected.Count, result.Count);

      for (int i = 0; i < expected.Count; ++i)
      {
        Assert.Equal(expected[i].RoleId, result[i].RoleId);

        Assert.Equal(expected[i].Name, result[i].Name);
      }
    }

    [Fact]
    public async Task UpdateUserRoleShouldUpdateTempRole()
    {
      UserService userService = new UserService(GetDbContext(), _otpServiceMock.Object, _userRepoMock.Object,
          _passwordHasherMock.Object, _employeeRepoMock.Object);

      //Set up employee repo
      int userId = 1;
      User oldUser = new User
      {
        UserId = userId,
        Role = UserRole.NormalUser,
        TempRole = UserRole.SuperUser
      };

      /// Get an Employee By Id should exist
      _userRepoMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(oldUser);
      Assert.NotNull(oldUser);
      /// TOTP should fire-> TOTPService will it's own tests
      User newUser = new User
      {
        UserId = oldUser.UserId,
        Role = oldUser.Role,
        TempRole = UserRole.NormalUser
      };


      _userRepoMock.Setup(r => r.UpdateUserAsync(userId, oldUser)).ReturnsAsync(newUser);

      //user.Role!=user.TempRole
      UpdateUserRoleRequestDto oldUserDto = new UpdateUserRoleRequestDto
      {
        RoleId = (int)newUser.TempRole
      };
      var result = await userService.UpdateUserRoleAsync(userId, oldUserDto);

      Assert.Equal(newUser, result);
    }

    [Theory]
    [MemberData(nameof(UserTestData.SetRandomUsersAndEmployees),
     MemberType = typeof(UserTestData))]
    public async Task OrganiseSuperUsersAsyncShouldReturnSuperUserEmployeeId(List<User> randomUsers, List<Employee> randomEmployees, List<string> expectedEmployeeIds)
    {
      //Arrange
      UserService userService = new UserService(GetDbContext(), _otpServiceMock.Object, _userRepoMock.Object,
         _passwordHasherMock.Object, _employeeRepoMock.Object);

      //Get all the users
      _userRepoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(randomUsers);

      for (int i = 0; i < randomUsers.Count; ++i)
        _employeeRepoMock.Setup(r => r.GetEmployeeByEmailAsync(randomUsers[i].Email)).ReturnsAsync(randomEmployees[i]);

      //Action
      var users = await userService.GetAllUsersAsync();

      Assert.NotNull(users);

      var results = await userService.OrganiseSuperUsersAsync();

      for (int i = 0; i < expectedEmployeeIds.Count; ++i)
      {
        Assert.Equal(expectedEmployeeIds, results);
      }
    }

  }
}
