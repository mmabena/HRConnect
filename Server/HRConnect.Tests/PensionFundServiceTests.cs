using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using HRConnect.Api.Repository;
using HRConnect.Api.Services;
using Moq;
using Xunit;

namespace HRConnect.Tests
{
  public class PensionFundServiceTests
  {
    private readonly Mock<IPensionFundRepository> _fundRepoMock;
    private readonly Mock<IPensionOptionRepository> _optionRepoMock;
    private readonly Mock<IEmployeePensionRepository> _employeeRepoMock;
    private readonly PensionFundService _service;

    public PensionFundServiceTests()
    {
      _fundRepoMock = new Mock<IPensionFundRepository>();
      _optionRepoMock = new Mock<IPensionOptionRepository>();
      _employeeRepoMock = new Mock<IEmployeePensionRepository>();

      _service = new PensionFundService(
          _fundRepoMock.Object,
          _optionRepoMock.Object,
          _employeeRepoMock.Object
      );
    }

    [Fact]
    public async Task AddPensionFundReturnsSuccess()
    {
      PensionFund fund = new PensionFund { EmployeeId = "E001", EmployeeName = "John Doe" };
      _fundRepoMock.Setup(r => r.AddPensionFundAsync(fund, CancellationToken.None))
                   .Returns(Task.CompletedTask);
      _fundRepoMock.Setup(r => r.SaveChangesAsync(CancellationToken.None))
                   .Returns(Task.CompletedTask);

      ServiceResult result = await _service.AddPensionFundAsync(fund, CancellationToken.None);

      Assert.True(result.IsSuccess);
      Assert.Equal("Fund added successfully.", result.Message);
    }

    [Fact]
    public async Task AddPensionOptionFailsWhenPercentageOutOfRange()
    {
      PensionOption option = new PensionOption { ContributionPercentage = 20 };

      ServiceResult result = await _service.AddPensionOptionAsync(option, CancellationToken.None);

      Assert.False(result.IsSuccess);
      Assert.Equal("Percentage must be between 0 and 15.", result.Message);
    }

    [Fact]
    public void CalculatePensionDeductionReturnsCorrectAmount()
    {
      PensionOption option = new PensionOption { ContributionPercentage = 10 };
      decimal salary = 1000;

      decimal amount = _service.CalculatePensionDeduction(salary, option);

      Assert.Equal(100, amount);
    }

    [Fact]
    public async Task RecordEmployeePensionSelectionFailsForNonPermanentEmployee()
    {
      Employee employee = new Employee
      {
        EmployeeId = "E002",
        EmploymentStatus = EmploymentStatus.Contract,
        MonthlySalary = 5000
      };
      PensionOption option = new PensionOption { PensionOptionId = 1, ContributionPercentage = 5 };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId, CancellationToken.None))
                       .ReturnsAsync(employee);
      _optionRepoMock.Setup(r => r.GetPensionOptionByIdAsync(option.PensionOptionId, CancellationToken.None))
                     .ReturnsAsync(option);

      ServiceResult result = await _service.RecordEmployeePensionSelectionAsync(employee.EmployeeId, option.PensionOptionId, CancellationToken.None);

      Assert.False(result.IsSuccess);
      Assert.Equal("Only permanent employees may select a pension option.", result.Message);
    }

    [Fact]
    public async Task RecordEmployeePensionSelectionSucceedsForPermanentEmployee()
    {
      Employee employee = new Employee
      {
        EmployeeId = "E003",
        EmploymentStatus = EmploymentStatus.Permanent,
        MonthlySalary = 5000,
        Name = "Jane Doe"
      };
      PensionOption option = new PensionOption { PensionOptionId = 2, ContributionPercentage = 10 };

      _employeeRepoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId, CancellationToken.None))
                       .ReturnsAsync(employee);
      _optionRepoMock.Setup(r => r.GetPensionOptionByIdAsync(option.PensionOptionId, CancellationToken.None))
                     .ReturnsAsync(option);
      _fundRepoMock.Setup(r => r.AddOrUpdatePensionFundAsync(It.IsAny<PensionFund>(), CancellationToken.None))
                   .Returns(Task.CompletedTask);
      _fundRepoMock.Setup(r => r.SaveChangesAsync(CancellationToken.None))
                   .Returns(Task.CompletedTask);

      ServiceResult result = await _service.RecordEmployeePensionSelectionAsync(employee.EmployeeId, option.PensionOptionId, CancellationToken.None);

      Assert.True(result.IsSuccess);
      Assert.Equal("Pension option selected and pension fund created.", result.Message);
    }
  }
}

