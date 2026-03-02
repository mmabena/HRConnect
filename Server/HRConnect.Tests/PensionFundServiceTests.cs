using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using HRConnect.Api.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HRConnect.Tests
{
  public class PensionFundServiceTests
  {
    private readonly Mock<IPensionRepository> _repoMock;
    private readonly PensionFundService _service;

    public PensionFundServiceTests()
    {
      _repoMock = new Mock<IPensionRepository>();
      _service = new PensionFundService(_repoMock.Object);
    }

    [Fact]
    public async Task AddPensionFund_ReturnsSuccess()
    {
      var fund = new PensionFund { EmployeeId = "E001", EmployeeName = "John Doe" };
      _repoMock.Setup(r => r.AddPensionFundAsync(fund)).Returns(Task.CompletedTask);

      var result = await _service.AddPensionFundAsync(fund);

      Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddPensionOption_Fails_WhenPercentageOutOfRange()
    {
      var option = new PensionOption { ContributionPercentage = 20 };

      var result = await _service.AddPensionOptionAsync(option);

      Assert.False(result.IsSuccess);
    }

    [Fact]
    public void CalculatePensionDeduction_ReturnsCorrectAmount()
    {
      var option = new PensionOption { ContributionPercentage = 10 };
      decimal salary = 1000;

      var amount = _service.CalculatePensionDeduction(salary, option);

      Assert.Equal(100, amount);
    }

    [Fact]
    public async Task RecordEmployeePensionSelection_Fails_ForNonPermanentEmployee()
    {
      var employee = new Employee
      {
        EmployeeId = "E002",
        EmploymentStatus = EmploymentStatus.Contract,
        MonthlySalary = 5000
      };
      var option = new PensionOption { PensionOptionId = 1, ContributionPercentage = 5 };

      _repoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
      _repoMock.Setup(r => r.GetPensionOptionByIdAsync(option.PensionOptionId)).ReturnsAsync(option);

      var result = await _service.RecordEmployeePensionSelectionAsync(employee.EmployeeId, option.PensionOptionId);

      Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RecordEmployeePensionSelection_Succeeds_ForPermanentEmployee()
    {
      var employee = new Employee
      {
        EmployeeId = "E003",
        EmploymentStatus = EmploymentStatus.Permanent,
        MonthlySalary = 5000
      };
      var option = new PensionOption { PensionOptionId = 2, ContributionPercentage = 10 };

      _repoMock.Setup(r => r.GetEmployeeByIdAsync(employee.EmployeeId)).ReturnsAsync(employee);
      _repoMock.Setup(r => r.GetPensionOptionByIdAsync(option.PensionOptionId)).ReturnsAsync(option);
      _repoMock.Setup(r => r.AddOrUpdatePensionFundAsync(It.IsAny<PensionFund>())).Returns(Task.CompletedTask);
      _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

      var result = await _service.RecordEmployeePensionSelectionAsync(employee.EmployeeId, option.PensionOptionId);

      Assert.True(result.IsSuccess);
    }
  }
}