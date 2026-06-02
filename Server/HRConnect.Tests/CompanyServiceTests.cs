namespace HRConnect.Tests
{
  using Xunit;
  using Moq;
  using Microsoft.AspNetCore.DataProtection;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.SignalR;
  using HRConnect.Api.Hubs;
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
    private readonly Mock<IHubContext<CompanyHub>> _companyHubContextMock;
    private readonly Mock<IHubClients> _hubClientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly CompanyService _companyService;

    public CompanyServiceTests()
    {
      _companyRepoMock = new Mock<ICompanyRepository>();

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
      _context = new ApplicationDBContext(options, mockProtector.Object);

      _companyHubContextMock = new Mock<IHubContext<CompanyHub>>();
      _hubClientsMock = new Mock<IHubClients>();
      _clientProxyMock = new Mock<IClientProxy>();

      _hubClientsMock
          .Setup(clients => clients.All)
          .Returns(_clientProxyMock.Object);

      _companyHubContextMock
          .Setup(x => x.Clients)
          .Returns(_hubClientsMock.Object);

      _companyService = new CompanyService(
          _context,
          _companyRepoMock.Object,
          _companyHubContextMock.Object
      );
    }

    [Fact]
    public async Task CreateCompanyAsync_ValidInput_ReturnsCorrectCompany()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "Duplicate Systems",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        ContactNumber = "0123456789",
        VATNumber = "VAT1234567",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      _companyRepoMock.Setup(r => r.GetAllCompanyIdsWithPrefix("DUP"))
          .ReturnsAsync(new List<string> { "DUP001" });

      Company? savedcompany = null;

      _companyRepoMock.Setup(r => r.CreateCompanyAsync(It.IsAny<Company>()))
          .ReturnsAsync((Company c) =>
          {
            savedcompany = c;
            return c;
          });

      // Act
      var result = await _companyService.CreateCompanyAsync(companyRequestDto);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Duplicate Systems", result.CompanyName);
      Assert.Equal("DUP002", savedcompany!.CompanyId);
    }

    [Fact]
    public async Task CreateCompanyAsync_MissingCompanyName_ThrowsValidationException()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        ContactNumber = "0123456789",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      // Act & Assert
      await Assert.ThrowsAsync<ValidationException>(() =>
          _companyService.CreateCompanyAsync(companyRequestDto));
    }

    [Fact]
    public async Task CreateCompanyAsync_DuplicateRegistrationNumber_ThrowsBusinessRuleException()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "Duplicate Systems",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        ContactNumber = "0123456789",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      _companyRepoMock.Setup(r => r.GetCompanyByRegNumberAsync(companyRequestDto.RegistrationNumber))
          .ReturnsAsync(new Company { RegistrationNumber = companyRequestDto.RegistrationNumber });

      // Act & Assert
      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _companyService.CreateCompanyAsync(companyRequestDto));
    }

    [Fact]
    public async Task CreateCompanyAsync_DeplicateUIFNumber_ThrowsBusinessRuleException()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "Duplicate Systems",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        ContactNumber = "0123456789",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      _companyRepoMock.Setup(r => r.GetCompanyByUIFAsync(companyRequestDto.UIFNumber))
          .ReturnsAsync(new Company { UIFNumber = companyRequestDto.UIFNumber });
      // Act & Assert
      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _companyService.CreateCompanyAsync(companyRequestDto));
    }

    [Fact]
    public async Task CreateCompanyAsync_DeplicateVATNumber_ThrowsBusinessRuleException()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "Duplicate Systems",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        VATNumber = "VAT1254785",
        ContactNumber = "0123456789",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      _companyRepoMock.Setup(r => r.GetCompanyByVATAsync(companyRequestDto.VATNumber))
          .ReturnsAsync(new Company { VATNumber = companyRequestDto.VATNumber });
      // Act & Assert
      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _companyService.CreateCompanyAsync(companyRequestDto));
    }

    [Fact]
    public async Task CreateCompanyAsync_DeplicateContactNumber_ThrowsBusinessRuleException()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "Duplicate Systems",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        VATNumber = "VAT1254785",
        ContactNumber = "0123456789",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      _companyRepoMock.Setup(r => r.GetCompanyByContactNumberAsync(companyRequestDto.ContactNumber))
          .ReturnsAsync(new Company { ContactNumber = companyRequestDto.ContactNumber });
      // Act & Assert
      await Assert.ThrowsAsync<BusinessRuleException>(() =>
          _companyService.CreateCompanyAsync(companyRequestDto));
    }


    [Fact]
    public async Task CreateCompanyAsync_GeneratesIncrementedCompanyId()
    {
      // Arrange
      var companyRequestDto = new CreateCompanyRequestDto
      {
        CompanyName = "Duplicate Systems",
        RegistrationNumber = "12345678901234",
        UIFNumber = "1234567890",
        ContactNumber = "0123456789",
        CompanyAddress = "123 Real Lane, Johannesburg"
      };

      _companyRepoMock.Setup(r => r.GetAllCompanyIdsWithPrefix("DUP"))
          .ReturnsAsync(new List<string> { "DUP001", "DUP002" });

      Company? capturedCompany = null;

      _companyRepoMock.Setup(r => r.CreateCompanyAsync(It.IsAny<Company>()))
          .ReturnsAsync((Company c) =>
          {
            capturedCompany = c;
            return c;
          });
      // Act
      await _companyService.CreateCompanyAsync(companyRequestDto);
      // Assert
      Assert.NotNull(capturedCompany);
      Assert.Equal("DUP003", capturedCompany.CompanyId);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }

  }
}