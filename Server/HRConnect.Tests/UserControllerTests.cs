namespace HRConnect.Tests
{
  using System;
  using System.Threading.Tasks;
  using HRConnect.Api.Controllers;
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Mvc;
  using Moq;

  public class UserControllerTests
  {
    [Fact]
    public async Task UpdateUserRoleReturnsOkWithUpdatedUser()
    {
      var userServiceMock = new Mock<IUserService>();
      var request = new UpdateUserRoleRequestDto { RoleId = (int)UserRole.SuperUser };

      userServiceMock
        .Setup(service => service.UpdateUserRoleAsync(12, request))
        .ReturnsAsync(new User
        {
          UserId = 12,
          Email = "user@singular.co.za",
          Role = UserRole.SuperUser,
          PasswordHash = "hashed",
        });

      var controller = new UserController(userServiceMock.Object);

      var result = await controller.UpdateUserRole(12, request);

      var okResult = Assert.IsType<OkObjectResult>(result);
      var dto = Assert.IsType<UserRegisterDto>(okResult.Value);
      Assert.Equal(12, dto.UserId);
      Assert.Equal("user@singular.co.za", dto.Email);
      Assert.Equal("SuperUser", dto.Role);
      Assert.Equal((int)UserRole.SuperUser, dto.RoleId);
    }

    [Fact]
    public async Task UpdateUserRoleReturnsNotFoundWhenUserMissing()
    {
      var userServiceMock = new Mock<IUserService>();
      var request = new UpdateUserRoleRequestDto { RoleId = (int)UserRole.SuperUser };

      userServiceMock
        .Setup(service => service.UpdateUserRoleAsync(99, request))
        .ReturnsAsync((User?)null);

      var controller = new UserController(userServiceMock.Object);

      var result = await controller.UpdateUserRole(99, request);

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateUserRoleReturnsValidationProblemForInvalidRole()
    {
      var userServiceMock = new Mock<IUserService>();
      var request = new UpdateUserRoleRequestDto { RoleId = 99 };

      userServiceMock
        .Setup(service => service.UpdateUserRoleAsync(12, request))
        .ThrowsAsync(new ArgumentException("Invalid role id."));

      var controller = new UserController(userServiceMock.Object);

      var result = await controller.UpdateUserRole(12, request);

      var objectResult = Assert.IsType<ObjectResult>(result);

      var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
      Assert.Contains("Validation", problemDetails.Errors.Keys);
      Assert.Contains("Invalid role id.", problemDetails.Errors["Validation"]);
    }

    [Fact]
    public async Task UpdateEmployeeUserRoleReturnsOkWithUpdatedUser()
    {
      var userServiceMock = new Mock<IUserService>();
      var request = new UpdateUserRoleRequestDto { RoleId = (int)UserRole.SuperUser };

      userServiceMock
        .Setup(service => service.UpdateEmployeeUserRoleAsync("EMP001", request))
        .ReturnsAsync(new User
        {
          UserId = 15,
          Email = "employee@singular.co.za",
          Role = UserRole.SuperUser,
          PasswordHash = "hashed",
        });

      var controller = new UserController(userServiceMock.Object);

      var result = await controller.UpdateEmployeeUserRole("EMP001", request);

      var okResult = Assert.IsType<OkObjectResult>(result);
      var dto = Assert.IsType<UserRegisterDto>(okResult.Value);
      Assert.Equal(15, dto.UserId);
      Assert.Equal("employee@singular.co.za", dto.Email);
      Assert.Equal("SuperUser", dto.Role);
      Assert.Equal((int)UserRole.SuperUser, dto.RoleId);
    }

    [Fact]
    public async Task UpdateEmployeeUserRoleReturnsNotFoundWhenEmployeeMissing()
    {
      var userServiceMock = new Mock<IUserService>();
      var request = new UpdateUserRoleRequestDto { RoleId = (int)UserRole.SuperUser };

      userServiceMock
        .Setup(service => service.UpdateEmployeeUserRoleAsync("EMP404", request))
        .ReturnsAsync((User?)null);

      var controller = new UserController(userServiceMock.Object);

      var result = await controller.UpdateEmployeeUserRole("EMP404", request);

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateEmployeeUserRoleReturnsValidationProblemForInvalidRole()
    {
      var userServiceMock = new Mock<IUserService>();
      var request = new UpdateUserRoleRequestDto { RoleId = -1 };

      userServiceMock
        .Setup(service => service.UpdateEmployeeUserRoleAsync("EMP001", request))
        .ThrowsAsync(new ArgumentException("Invalid role id."));

      var controller = new UserController(userServiceMock.Object);

      var result = await controller.UpdateEmployeeUserRole("EMP001", request);

      var objectResult = Assert.IsType<ObjectResult>(result);

      var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
      Assert.Contains("Validation", problemDetails.Errors.Keys);
      Assert.Contains("Invalid role id.", problemDetails.Errors["Validation"]);
    }
  }
}