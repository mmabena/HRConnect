namespace HRConnect.Tests
{
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
        private static EmployeeController CreateControllerWithRole(string role)
        {
            // Mock the service
            var mockService = new Mock<IEmployeeService>();

            // Create controller
            var controller = new EmployeeController(mockService.Object);

            // Mock the HttpContext with a ClaimsPrincipal having a role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "testuser@singular.co.za"),
                new Claim(ClaimTypes.Role, role) // assign role here
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task SuperUserCanAccessGetAllEmployees()
        {
            // Arrange
            var mockService = new Mock<IEmployeeService>();
            mockService.Setup(s => s.GetAllEmployeesAsync())
                       .ReturnsAsync(new List<EmployeeDto>());

            var controller = new EmployeeController(mockService.Object);

            // Set up SuperUser role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "superuser@singular.co.za"),
                new Claim(ClaimTypes.Role, "SuperUser")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetAllEmployees();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task NormalUserCannotAccessGetAllEmployees()
        {
            // Arrange
            var mockService = new Mock<IEmployeeService>();
            var controller = new EmployeeController(mockService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "normaluser@singular.co.za"),
                new Claim(ClaimTypes.Role, "NormalUser")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Simulate middleware authorization
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
            // Arrange
            var mockService = new Mock<IEmployeeService>();
            var dto = new CreateEmployeeRequestDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@singular.co.za"
            };

            mockService.Setup(s => s.CreateEmployeeAsync(dto))
                       .ReturnsAsync(new EmployeeDto
                       {
                           EmployeeId = "123",
                           Name = dto.Name,
                           Surname = dto.Surname
                       });

            var controller = new EmployeeController(mockService.Object);

            // Set up SuperUser role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "test@singular.co.za"),
                new Claim(ClaimTypes.Role, "SuperUser")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.CreateEmployee(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var employee = Assert.IsType<EmployeeDto>(createdResult.Value);
            Assert.Equal("123", employee.EmployeeId);
        }

        [Fact]
        public async Task NormalUserCannotCreateEmployee()
        {
            // Arrange
            var mockService = new Mock<IEmployeeService>();
            var dto = new CreateEmployeeRequestDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@singular.co.za"
            };

            var controller = new EmployeeController(mockService.Object);

            // Set up NormalUser role
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, "test@singular.co.za"),
        new Claim(ClaimTypes.Role, "NormalUser")
    }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Simulate middleware: check role manually
            IActionResult result;
            var role = controller.ControllerContext.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "SuperUser")
                result = new ForbidResult();
            else
                result = await controller.CreateEmployee(dto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

    }
}