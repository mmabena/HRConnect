namespace HRConnect.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Models;
    using HRConnect.Api.Services;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Utils.Security;
    using HRConnect.Api.Utils.BankingDetailsValidation;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Xunit;

    public class BankDetailServiceTests
    {
        private readonly Mock<IBankingDetailRepository> _bankingDetailRepoMock;
        private readonly Mock<IEmployeeRepository> _employeeRepoMock;
        private readonly Mock<IEncryptionService> _encryptionServiceMock;
        private readonly Mock<ILogger<BankingDetailService>> _loggerMock;
        private readonly BankingDetailService _bankingDetailService;

        private readonly Mock<IConfiguration> _configurationMock;
        private readonly HashingHelper _hashingHelper;

        public BankDetailServiceTests()
        {
            _bankingDetailRepoMock = new Mock<IBankingDetailRepository>();
            _employeeRepoMock = new Mock<IEmployeeRepository>();
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _loggerMock = new Mock<ILogger<BankingDetailService>>();

            _configurationMock = new Mock<IConfiguration>();

            _configurationMock
             .Setup(c => c["EncryptionSettings:Key"])
             .Returns(Convert.ToBase64String(
             System.Text.Encoding.UTF8.GetBytes("TestSecretKey123")));
            _hashingHelper = new HashingHelper(_configurationMock.Object);


            // FIX: constructor updated
            _bankingDetailService = new BankingDetailService(
                _bankingDetailRepoMock.Object,
                _encryptionServiceMock.Object,
                _loggerMock.Object,
                _employeeRepoMock.Object,
                _hashingHelper

            );
        }

        // ======================================================
        // GET ALL
        // ======================================================
        [Fact]
        public async Task GetAllBankingDetailsAsync_ReturnsList()
        {
            var bankingDetails = new List<BankingDetail>
            {
                new BankingDetail
                {
                    BankingDetailsId = 1,
                    Name = "John",
                    Surname = "Doe",
                    AccountNumberEncrypted = "enc1"
                },
                new BankingDetail
                {
                    BankingDetailsId = 2,
                    Name = "Jane",
                    Surname = "Smith",
                    AccountNumberEncrypted = "enc2"
                }
            };

            _bankingDetailRepoMock
                .Setup(r => r.GetAllBankingDetailsAsync())
                .ReturnsAsync(bankingDetails);

            _encryptionServiceMock
                .Setup(e => e.Decrypt(It.IsAny<string>()))
                .Returns("1234567890");

            var result = await _bankingDetailService.GetAllBankingDetailsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ======================================================
        // GET BY EMPLOYEE
        // ======================================================
        [Fact]
        public async Task GetBankingDetailsByEmployeeIdAsync_ReturnsData()
        {
            var employeeId = "E001";

            var entity = new BankingDetail
            {
                BankingDetailsId = 1,
                EmployeeId = employeeId,
                Name = "John",
                Surname = "Doe",
                AccountNumberEncrypted = "enc"
            };

            _bankingDetailRepoMock
                .Setup(r => r.GetBankingDetailsByEmployeeIdAsync(employeeId))
                .ReturnsAsync(entity);

            _encryptionServiceMock
                .Setup(e => e.Decrypt("enc"))
                .Returns("1234567890");

            var result = await _bankingDetailService
                .GetBankingDetailsByEmployeeIdAsync(employeeId);

            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
        }

        // ======================================================
        // CREATE
        // ======================================================
        [Fact]
        public async Task CreateBankingDetailsAsync_ShouldCreate_whenValid()
        {
            var dto = new CreateBankingDetailDto
            {
                EmployeeId = "E001",
                Name = "John",
                Surname = "Doe",
                AccountNumber = "1234567890",
                BankBranchCodeId = 1,
                BankName = BankName.FNB,
                AccountType = AccountType.Savings
            };

            var employee = new Employee
            {
                EmployeeId = "E001",
                EmploymentStatus = EmploymentStatus.Permanent
            };

            _employeeRepoMock
                .Setup(r => r.GetEmployeeByIdAsync(dto.EmployeeId))
                .ReturnsAsync(employee);

            _bankingDetailRepoMock
                .Setup(r => r.GetBankingDetailsByEmployeeIdAsync(dto.EmployeeId))
                .ReturnsAsync((BankingDetail)null);

            _encryptionServiceMock
                .Setup(e => e.Encrypt(dto.AccountNumber))
                .Returns("encrypted");

            _bankingDetailRepoMock
                .Setup(r => r.CreateBankingDetailsAsync(It.IsAny<BankingDetail>()))
                .ReturnsAsync(new BankingDetail
                {
                    BankingDetailsId = 1,
                    Name = dto.Name,
                    Surname = dto.Surname,
                    AccountNumberEncrypted = "encrypted"
                });

            _encryptionServiceMock
                .Setup(e => e.Decrypt("encrypted"))
                .Returns(dto.AccountNumber);

            var result = await _bankingDetailService.CreateBankingDetailsAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
        }

        // ======================================================
        // UPDATE
        // ======================================================
        [Fact]
        public async Task UpdateBankingDetailsAsync_UpdatesSuccessfully()
        {
            var employeeId = "E001";

            var existing = new BankingDetail
            {
                BankingDetailsId = 1,
                EmployeeId = employeeId,
                AccountNumberEncrypted = "enc",
                IsLocked = false
            };

            var dto = new UpdateBankingDetailDto
            {
                AccountNumber = "9999999999",
                BankBranchCodeId = 1,
                BankName = BankName.FNB,
                AccountType = AccountType.Savings
            };

            _employeeRepoMock
                .Setup(r => r.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(new Employee { EmployeeId = employeeId });

            _bankingDetailRepoMock
                .Setup(r => r.GetBankingDetailsByEmployeeIdAsync(employeeId))
                .ReturnsAsync(existing);

            _encryptionServiceMock
                .Setup(e => e.Encrypt(dto.AccountNumber))
                .Returns("newEnc");

            _bankingDetailRepoMock
                .Setup(r => r.UpdateBankingDetailsAsync(It.IsAny<BankingDetail>()))
                .ReturnsAsync((BankingDetail updated) => updated);

            _encryptionServiceMock
                .Setup(e => e.Decrypt("newEnc"))
                .Returns(dto.AccountNumber);

            var result = await _bankingDetailService
                .UpdateBankingDetailsAsync(employeeId, dto);

            Assert.NotNull(result);
        }

        // ======================================================
        // LOCK ALL
        // ======================================================
        [Fact]
        public async Task LockAllBankingDetailsAsync_LocksAllRecords()
        {
            var list = new List<BankingDetail>
            {
                new BankingDetail { IsLocked = false },
                new BankingDetail { IsLocked = false }
            };

            _bankingDetailRepoMock
                .Setup(r => r.GetAllBankingDetailsAsync())
                .ReturnsAsync(list);

            _bankingDetailRepoMock
                .Setup(r => r.UpdateBankingDetailsAsync(It.IsAny<BankingDetail>()))
                .ReturnsAsync((BankingDetail updated) => updated);

            await _bankingDetailService.LockAllBankingDetailsAsync();

            Assert.All(list, item => Assert.True(item.IsLocked));
        }
    }
}