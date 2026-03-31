namespace HRConnect.Tests.Services
{
  using System;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Moq;

  public class UserServiceEmployeeSyncTests : IDisposable
  {
    private readonly ApplicationDBContext _context;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly UserService _userService;

    public UserServiceEmployeeSyncTests()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

      _context = new ApplicationDBContext(options);
      _userRepositoryMock = new Mock<IUserRepository>(MockBehavior.Strict);
      _passwordHasherMock = new Mock<IPasswordHasher<User>>();
      _passwordHasherMock
        .Setup(hasher => hasher.HashPassword(It.IsAny<User>(), It.IsAny<string>()))
        .Returns((User user, string _) => $"hashed::{user.Email}");

      _userService = new UserService(
        _context,
        _userRepositoryMock.Object,
        _passwordHasherMock.Object);
    }

    [Fact]
    public async Task SyncEmployeeUsersAsyncCreatesMissingNormalUsersForEmployees()
    {
      await _context.Employees.AddRangeAsync(
        CreateEmployee("EMP001", "alex.mpho@singular.co.za"),
        CreateEmployee("EMP002", "nomsa.dube@singular.co.za"));

      await _context.Users.AddAsync(new User
      {
        Email = "alex.mpho@singular.co.za",
        PasswordHash = "existing-hash",
        Role = UserRole.SuperUser,
        CreatedAt = DateTime.UtcNow,
      });

      await _context.SaveChangesAsync();

      await _userService.SyncEmployeeUsersAsync();

      var users = await _context.Users.ToListAsync();

      Assert.Equal(2, users.Count);
      Assert.Contains(users, user => user.Email == "alex.mpho@singular.co.za" && user.Role == UserRole.SuperUser);
      Assert.Contains(users, user => user.Email == "nomsa.dube@singular.co.za" && user.Role == UserRole.NormalUser);
    }

    [Fact]
    public async Task UpdateEmployeeUserRoleAsyncCreatesMissingUserAndAppliesRequestedRole()
    {
      await _context.Employees.AddAsync(CreateEmployee("EMP777", "new.employee@singular.co.za"));
      await _context.SaveChangesAsync();

      var result = await _userService.UpdateEmployeeUserRoleAsync("EMP777", new UpdateUserRoleRequestDto
      {
        RoleId = (int)UserRole.SuperUser,
      });

      Assert.NotNull(result);
      Assert.Equal("new.employee@singular.co.za", result!.Email);
      Assert.Equal(UserRole.SuperUser, result.Role);

      var persistedUser = await _context.Users.SingleAsync(user => user.Email == "new.employee@singular.co.za");
      Assert.Equal(UserRole.SuperUser, persistedUser.Role);
    }

    [Fact]
    public async Task UpdateEmployeeUserRoleAsyncReturnsNullWhenEmployeeDoesNotExist()
    {
      var result = await _userService.UpdateEmployeeUserRoleAsync("EMP404", new UpdateUserRoleRequestDto
      {
        RoleId = (int)UserRole.SuperUser,
      });

      Assert.Null(result);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }

    private static Employee CreateEmployee(string employeeId, string email)
    {
      return new Employee
      {
        EmployeeId = employeeId,
        Title = Title.Mr,
        Name = "Test",
        Surname = "Employee",
        Gender = Gender.Male,
        ContactNumber = "0123456789",
        TaxNumber = "1234567890",
        Email = email,
        PhysicalAddress = "1 Main Road",
        City = "Johannesburg",
        ZipCode = "2000",
        Nationality = "South African",
        DateOfBirth = new DateOnly(1990, 1, 1),
        StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
        Branch = Branch.Johannesburg,
        MonthlySalary = 25000m,
        PositionId = 1,
        EmploymentStatus = EmploymentStatus.Permanent,
        ProfileImage = "profile.jpg",
      };
    }
  }
}