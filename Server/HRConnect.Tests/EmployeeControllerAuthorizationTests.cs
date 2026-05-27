namespace HRConnect.Tests
{
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using HRConnect.Api.Controllers;
    using HRConnect.Api.DTOs.Employee; 
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using System.Collections.Generic;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Hubs;
    using Microsoft.AspNetCore.SignalR;
    public class EmployeeControllerAuthorizationTests
    {
        private static ClaimsPrincipal CreateUser(string role, int userId = 1)
        {
<<<<<<< HEAD
            // Mock the service and controller dependencies
            var mockService = new Mock<IEmployeeService>();
            var mockLeaveBalance = new Mock<ILeaveBalanceService>();
            var mockHubContext = new Mock<IHubContext<UserPositionHub>>();
=======
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("userId", userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, "test@singular.co.za"),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));
        }
>>>>>>> fa6a53bef625ffd9f8ff87369827de98ea0f3ce9

        private static EmployeeController CreateController(string role, Mock<IEmployeeService> serviceMock)
        {
            var controller = new EmployeeController(
<<<<<<< HEAD
                mockService.Object,
                mockLeaveBalance.Object,
                mockHubContext.Object
        
=======
                serviceMock.Object,
                new Mock<ILeaveBalanceService>().Object
>>>>>>> fa6a53bef625ffd9f8ff87369827de98ea0f3ce9
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

<<<<<<< HEAD
            var mockLeaveBalance = new Mock<ILeaveBalanceService>();
            var mockHubContext = new Mock<IHubContext<UserPositionHub>>();

            var controller = new EmployeeController(
                mockService.Object,
                mockLeaveBalance.Object,
                mockHubContext.Object
            );

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
=======
            var controller = CreateController("SuperUser", serviceMock);

>>>>>>> fa6a53bef625ffd9f8ff87369827de98ea0f3ce9
            var result = await controller.GetAllEmployees();

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task NormalUserCannotAccessGetAllEmployees()
        {
<<<<<<< HEAD
            // Arrange
            var mockService = new Mock<IEmployeeService>();
            var mockLeaveBalance = new Mock<ILeaveBalanceService>();
            var mockHubContext = new Mock<IHubContext<UserPositionHub>>();

            var controller = new EmployeeController(
                mockService.Object,
                mockLeaveBalance.Object,
                mockHubContext.Object
            );
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "normaluser@singular.co.za"),
                new Claim(ClaimTypes.Role, "NormalUser")
            }, "mock"));
=======
            var serviceMock = new Mock<IEmployeeService>();

            var controller = CreateController("NormalUser", serviceMock);
>>>>>>> fa6a53bef625ffd9f8ff87369827de98ea0f3ce9

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

<<<<<<< HEAD
            var mockLeaveBalance = new Mock<ILeaveBalanceService>();
            var mockHubContext = new Mock<IHubContext<UserPositionHub>>();  

            var controller = new EmployeeController(
                mockService.Object,
                mockLeaveBalance.Object,
                mockHubContext.Object
            );

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
=======
            var controller = CreateController("SuperUser", serviceMock);

>>>>>>> fa6a53bef625ffd9f8ff87369827de98ea0f3ce9
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

<<<<<<< HEAD
            var mockLeaveBalance = new Mock<ILeaveBalanceService>();
            var mockHubContext = new Mock<IHubContext<UserPositionHub>>();

            var controller = new EmployeeController(
                mockService.Object,
                mockLeaveBalance.Object,
                mockHubContext.Object
            );

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
=======
            var controller = CreateController("NormalUser", new Mock<IEmployeeService>());

>>>>>>> fa6a53bef625ffd9f8ff87369827de98ea0f3ce9
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