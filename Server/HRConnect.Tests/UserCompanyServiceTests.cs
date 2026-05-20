namespace HRConnect.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using HRConnect.Api.Services;
    using Microsoft.AspNetCore.SignalR;
    using HRConnect.Api.Hubs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Mappers;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.UserCompany;
    using System.Linq;
    using Moq;
    public class UserCompanyServiceTests : IDisposable
    {
        private readonly Mock<IUserCompanyRepository> _userCompanyRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ICompanyRepository> _companyRepoMock;
        private readonly ApplicationDBContext _context;
        private readonly Mock<IHubContext<CompanyHub>> _companyHubContextMock;
        private readonly UserCompanyService _userCompanySerice;

        public UserCompanyServiceTests()
        {
            _companyRepoMock = new Mock<ICompanyRepository>();
            _userCompanyRepoMock = new Mock<IUserCompanyRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _companyHubContextMock = new Mock<IHubContext<CompanyHub>>();

            var oprions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(oprions);

            _userCompanySerice = new UserCompanyService(
                _context,
                _userCompanyRepoMock.Object,
                _userRepoMock.Object,
                _companyRepoMock.Object,
                _companyHubContextMock.Object
            );
        }

        [Fact]
        public async Task AssignCompanyToUserAsync_ValidAssignment_ReturnSuccessfulAssignment()
        {
            // Arrange 
            var userId = 1;

            var userCompanyRequestDto = new CreateUserCompanyDto
            {
                CompanyId = "DUP001",
                IsDefault = true
            };

            _userRepoMock
                .Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    UserId = userId,
                    Email = "james@singular.co.za",
                    Role = UserRole.SuperUser
                });

            _companyRepoMock
                .Setup(r => r.GetCompanyByIdAsync("DUP001"))
                .ReturnsAsync(new Company
                {
                    CompanyId = "DUP001",
                    CompanyName = "Duplicate Systems"
                });

            _userCompanyRepoMock
                .Setup(r => r.GetUserCompaniesByUserIdAsync(userId))
                .ReturnsAsync(new List<UserCompany>());

            UserCompany? createdUserCompany = null;

            _userCompanyRepoMock
                .Setup(r => r.CreateUserCompanyAsync(It.IsAny<UserCompany>()))
                .ReturnsAsync((UserCompany uc) =>
                {
                    createdUserCompany = uc;
                    return uc;
                });

            //Act
            await _userCompanySerice.AssignCompanyToUserAsync(userId, userCompanyRequestDto);

            //Assert
            Assert.NotNull(createdUserCompany);
            Assert.Equal("DUP001", createdUserCompany.CompanyId);
            Assert.True(createdUserCompany.IsDefault);

        }

        [Fact]
        public async Task SwitchCompanyAsync_UserNotLinkedToCompany_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 1;
            var companyId = "DUP001";

            _userCompanyRepoMock
                .Setup(r => r.UserCompanyExistsAsync(userId, companyId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _userCompanySerice.SwitchCompanyAsync(userId, companyId)
             );
        }

        [Fact]
        public async Task AssignCompanyToUserAsync_CompanyDoesNotExist_ThrowArgumentException()
        {
            // Arrange 
            var userId = 1;

            var userCompanyRequestDto = new CreateUserCompanyDto
            {
                CompanyId = "DUP001",
                IsDefault = true
            };

            _userRepoMock
                .Setup(r => r.GetUserByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    UserId = userId,
                    Email = "james@singular.co.za",
                    Role = UserRole.NormalUser
                });

            _companyRepoMock
                .Setup(r => r.GetCompanyByIdAsync("DUP001"))
                .ReturnsAsync((Company?)null);

            _userCompanyRepoMock
                .Setup(r => r.GetUserCompaniesByUserIdAsync(userId))
                .ReturnsAsync(new List<UserCompany>());

            //Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _userCompanySerice.AssignCompanyToUserAsync(userId, userCompanyRequestDto)
             );
        }
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}