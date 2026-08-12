using System.Collections.Generic;
using System.Threading.Tasks;
using HRConnect.Api.DTOs;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using HRConnect.Api.Services;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using HRConnect.Api.Repository;

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
      var fund = new PensionFund
      {
        EmployeeId = "E001",
        EmployeeName = "John Doe"
      };

      var result = await _service.AddPensionFundAsync(
          fund,
          CancellationToken.None);

      Assert.True(result.IsSuccess);
      Assert.Equal("Fund added successfully.", result.Message);
    }

    [Fact]
    public async Task AddPensionOptionFailsWhenPercentageOutOfRange()
    {
      var option = new PensionOption
      {
        ContributionPercentage = 20
      };

      var result = await _service.AddPensionOptionAsync(
          option,
          CancellationToken.None);

      Assert.False(result.IsSuccess);
      Assert.Contains("between 0 and 15", result.Message);
    }

    [Fact]
    public void CalculatePensionDeductionReturnsCorrectAmount()
    {
      var option = new PensionOption
      {
        ContributionPercentage = 10
      };

      decimal salary = 1000;

      var amount = _service.CalculatePensionDeduction(
          salary,
          option);

      Assert.Equal(100, amount);
    }

    [Fact]
    public async Task CreatePensionFund_Fails_WhenActiveFundAlreadyExists()
    {
      var dto = new CreatePensionFundDto
      {
        Name = "New Fund",
        Description = "Test",
        TaxCode = 4001
      };

      var existingFunds = new List<PensionFund>
    {
        new PensionFund
        {
            Name = "Existing Fund",
            IsActive = true
        }
    };

      _fundRepoMock.Setup(r => r.GetPensionFundsAsync(It.IsAny<CancellationToken>()))
       .ReturnsAsync(existingFunds);

      var result = await _service.CreatePensionFundAsync(
          dto,
          CancellationToken.None);

      Assert.False(result.IsSuccess);

      Assert.Contains(
          "active pension fund already exists",
          result.Message);
    }


    [Fact]
    public async Task CreatePensionFund_Succeeds_WhenNoActiveFundExists()
    {
      var dto = new CreatePensionFundDto
      {
        Name = "Discovery",
        Description = "Discovery Pension Fund",
        TaxCode = 4001
      };

      _fundRepoMock
          .Setup(r => r.GetPensionFundsAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(new List<PensionFund>());

      var result = await _service.CreatePensionFundAsync(
          dto,
          CancellationToken.None);

      Assert.True(result.IsSuccess);

      Assert.Contains(
          "created successfully",
          result.Message);
    }

    [Fact]
    public async Task RecordEmployeePensionSelectionSucceedsForPermanentEmployee()
    {
      // Employee
      var employee = new Employee
      {
        EmployeeId = "E003",
        Name = "John Doe",
        EmploymentStatus = EmploymentStatus.Permanent,
        MonthlySalary = 5000,
        Name = "Jane Doe"
      };

      // Pension Option
      var option = new PensionOption
      {
        PensionOptionId = 2,
        ContributionPercentage = 10
      };

      // Active Fund
      var activeFund = new PensionFund
      {
        Name = "Discovery Fund",
        Description = "Main Pension Fund",
        TaxCode = 4001,
        IsActive = true
      };

      // Employee lookup
      _employeeRepoMock
          .Setup(r => r.GetEmployeeByIdAsync(
              employee.EmployeeId,
              It.IsAny<CancellationToken>()))
          .ReturnsAsync(employee);

      // Option lookup
      _optionRepoMock
          .Setup(r => r.GetPensionOptionByIdAsync(
              option.PensionOptionId,
              It.IsAny<CancellationToken>()))
          .ReturnsAsync(option);

      // Active fund lookup
      _fundRepoMock
          .Setup(r => r.GetPensionFundsAsync(
              It.IsAny<CancellationToken>()))
          .ReturnsAsync(new List<PensionFund>
          {
            activeFund
          });

      // Employee does not already have a pension record
      _fundRepoMock
          .Setup(r => r.GetPensionFundByEmployeeIdAsync(
              employee.EmployeeId,
              It.IsAny<CancellationToken>()))
          .ReturnsAsync((PensionFund?)null);

      var result = await _service.RecordEmployeePensionSelectionAsync(
          employee.EmployeeId,
          option.PensionOptionId,
          CancellationToken.None);

      Assert.True(result.IsSuccess);
      Assert.Contains(
          "Pension option selected",
          result.Message);
    }

  }
}
