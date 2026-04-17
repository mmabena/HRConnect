namespace HRConnect.Tests
{
    using Xunit;
    using Moq;
    using HRConnect.Api.Services;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.Company;
    using HRConnect.Api.Data;
    using HRConnect.Api.Mappers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    public class CompanyServiceTests : IDisposable
    {
        private readonly Mock<ICompanyRepository> _companyRepoMock;
        private readonly ApplicationDBContext _context;
        private readonly CompanyService _companyService;

        public CompanyServiceTests()
        {
            _companyRepoMock = new Mock<ICompanyRepository>();

            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);

            _companyService = new CompanyService(
                _context,
                _companyRepoMock.Object
            );
        }

        [Fact]
        public async Task CreateCompanyAsync_ValidInput_ReturnsCreatedCompany()
        {
            var dto = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789",
                VATNumber = "VAT1234567"
            };

            _companyRepoMock.Setup(r => r.GetAllCompanyIdsWithPrefix("SIN"))
                .ReturnsAsync(new List<string>());

            _companyRepoMock.Setup(r => r.CreateCompanyAsync(It.IsAny<Company>()))
                .ReturnsAsync((Company c) => c);

            var result = await _companyService.CreateCompanyAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Singular Systems", result.CompanyName);
        }

        [Fact]
        public async Task CreateCompanyAsync_MissingCompanyName_ThrowsArgumentException()
        {
            var dto = new CreateCompanyRequestDto
            {
                CompanyName = "",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789"
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _companyService.CreateCompanyAsync(dto));
        }

        [Fact]
        public async Task CreateCompanyAsync_DuplicateRegistrationNumber_ThrowsInvalidOperationException()
        {
            var dto = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789"
            };

            _companyRepoMock.Setup(r => r.GetCompanyByRegNumberAsync(dto.RegistrationNumber))
                .ReturnsAsync(new Company { RegistrationNumber = dto.RegistrationNumber });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _companyService.CreateCompanyAsync(dto));
        }

        [Fact]
        public async Task CreateCompanyAsync_GeneratesIncrementedCompanyId()
        {
            var dto = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789"
            };

            _companyRepoMock.Setup(r => r.GetAllCompanyIdsWithPrefix("SIN"))
                .ReturnsAsync(new List<string> { "SIN001", "SIN002" });

            Company? capturedCompany = null;

            _companyRepoMock.Setup(r => r.CreateCompanyAsync(It.IsAny<Company>()))
                .ReturnsAsync((Company c) =>
                {
                    capturedCompany = c;
                    return c;
                });

            await _companyService.CreateCompanyAsync(dto);

            Assert.NotNull(capturedCompany);
            Assert.Equal("SIN003", capturedCompany.CompanyId);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}