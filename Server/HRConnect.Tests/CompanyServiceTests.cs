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
            var companyRequestDto = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789",
                VATNumber = "VAT1234567"
            };

            _companyRepoMock.Setup(r => r.GetAllCompanyIdsWithPrefix("SIN"))
                .ReturnsAsync(new List<string>{ "SIN001"});

            Company? savedcompany = null;

            _companyRepoMock.Setup(r => r.CreateCompanyAsync(It.IsAny<Company>()))
                .ReturnsAsync((Company c) =>
                {
                    savedcompany = c;
                    return c;
                });

            var result = await _companyService.CreateCompanyAsync(companyRequestDto);

            Assert.NotNull(result);
            Assert.Equal("Singular Systems", result.CompanyName);
            Assert.Equal("SIN002", savedcompany!.CompanyId);
        }

        [Fact]
        public async Task CreateCompanyAsync_MissingCompanyName_ThrowsArgumentException()
        {
            var companyRequestDto = new CreateCompanyRequestDto
            {
                CompanyName = "",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789"
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _companyService.CreateCompanyAsync(companyRequestDto));
        }

        [Fact]
        public async Task CreateCompanyAsync_DuplicateRegistrationNumber_ThrowsInvalidOperationException()
        {
            var companyRequestDto = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789"
            };

            _companyRepoMock.Setup(r => r.GetCompanyByRegNumberAsync(companyRequestDto.RegistrationNumber))
                .ReturnsAsync(new Company { RegistrationNumber = companyRequestDto.RegistrationNumber });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _companyService.CreateCompanyAsync(companyRequestDto));
        }

        [Fact]
        public async Task CreateCompanyAsync_DeplicateUIFNumber_ThrowsInvalidOperationException()
        {
            var companyRequestDto  = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                ContactNumber = "0123456789"
            };

            _companyRepoMock.Setup(r => r.GetCompanyByUIFAsync(companyRequestDto.UIFNumber))
                .ReturnsAsync(new Company { UIFNumber = companyRequestDto.UIFNumber });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _companyService.CreateCompanyAsync(companyRequestDto));
        }

        [Fact]
        public async Task CreateCompanyAsync_DeplicateVATNumber_ThrowsInvalidOperationException()
        {
            var companyRequestDto  = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                VATNumber = "VAT1254785",
                ContactNumber = "0123456789"
            };

            _companyRepoMock.Setup(r => r.GetCompanyByVATAsync(companyRequestDto.VATNumber))
                .ReturnsAsync(new Company { VATNumber = companyRequestDto.VATNumber });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _companyService.CreateCompanyAsync(companyRequestDto));
        }

        [Fact]
        public async Task CreateCompanyAsync_DeplicateContactNumber_ThrowsInvalidOperationException()
        {
            var companyRequestDto  = new CreateCompanyRequestDto
            {
                CompanyName = "Singular Systems",
                RegistrationNumber = "12345678901234",
                UIFNumber = "1234567890",
                VATNumber = "VAT1254785",
                ContactNumber = "0123456789"
            };

            _companyRepoMock.Setup(r => r.GetCompanyByContactNumberAsync(companyRequestDto.ContactNumber))
                .ReturnsAsync(new Company { ContactNumber = companyRequestDto.ContactNumber });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _companyService.CreateCompanyAsync(companyRequestDto));
        }

    

        [Fact]
        public async Task CreateCompanyAsync_GeneratesIncrementedCompanyId()
        {
            var companyRequestDto = new CreateCompanyRequestDto
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

            await _companyService.CreateCompanyAsync(companyRequestDto);

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