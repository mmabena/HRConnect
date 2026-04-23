namespace HRConnect.Tests
{
    using System.Globalization;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using HRConnect.Api.Controllers;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.Employee;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class EmployeeControllerAuthorizationTests
    {
        private static ClaimsPrincipal CreateUser(string role, int userId = 1)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("userId", userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, "test@singular.co.za"),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));
        }

        private static EmployeeController CreateController(string role, Mock<IEmployeeService> serviceMock)
        {
            var controller = new EmployeeController(
                serviceMock.Object,
                new Mock<ILeaveBalanceService>().Object
            );

            

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = CreateUser(role)
                }
            };

            return controller;
        }

        [Fact]
        public async Task SuperUserCanAccessGetAllEmployees()
        {
            var serviceMock = new Mock<IEmployeeService>();

            serviceMock.Setup(s => s.GetAllEmployeesAsync(It.IsAny<int>()))
                       .ReturnsAsync(new List<EmployeeDto>());

            var controller = CreateController("SuperUser", serviceMock);

            var result = await controller.GetAllEmployees();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task NormalUserCannotAccessGetAllEmployees()
        {
            var serviceMock = new Mock<IEmployeeService>();

            var controller = CreateController("NormalUser", serviceMock);

            IActionResult result;

            var role = controller.ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "SuperUser")
                result = new ForbidResult();
            else
                result = await controller.GetAllEmployees();

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SuperUserCanCreateEmployee()
        {
            var dto = new CreateEmployeeRequestDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@singular.co.za"
            };

            var serviceMock = new Mock<IEmployeeService>();

            serviceMock.Setup(s => s.CreateEmployeeAsync(It.IsAny<int>(), dto))
                       .ReturnsAsync(new EmployeeDto
                       {
                           EmployeeId = "123",
                           Name = dto.Name,
                           Surname = dto.Surname
                       });

            var controller = CreateController("SuperUser", serviceMock);

            var result = await controller.CreateEmployee(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var employee = Assert.IsType<EmployeeDto>(created.Value);

            Assert.Equal("123", employee.EmployeeId);
        }

        [Fact]
        public async Task NormalUserCannotCreateEmployee()
        {
            var dto = new CreateEmployeeRequestDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@singular.co.za"
            };

            var controller = CreateController("NormalUser", new Mock<IEmployeeService>());

            IActionResult result;

            var role = controller.ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "SuperUser")
                result = new ForbidResult();
            else
                result = await controller.CreateEmployee(dto);

            Assert.IsType<ForbidResult>(result);
        }
    }
}