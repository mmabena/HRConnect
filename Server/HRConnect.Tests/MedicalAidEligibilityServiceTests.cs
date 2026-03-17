namespace HRConnect.Tests;

using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using HRConnect.Api.Services;
using HRConnect.Api.Interfaces;
using HRConnect.Api.DTOs;
using HRConnect.Api.DTOs.Employee;
using HRConnect.Api.DTOs.MedicalOption;
using HRConnect.Api.Models;

public class MedicalAidEligibilityServiceTests
{
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IMedicalOptionRepository> _mockMedicalOptionRepository;
    private readonly MedicalAidEligibilityService _service;

    public MedicalAidEligibilityServiceTests()
    {
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockMedicalOptionRepository = new Mock<IMedicalOptionRepository>();
        _service = new MedicalAidEligibilityService(_mockEmployeeService.Object, _mockMedicalOptionRepository.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ConstructorWithValidDependenciesShouldInitializeService()
    {
        // Act
        var service = new MedicalAidEligibilityService(_mockEmployeeService.Object, _mockMedicalOptionRepository.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void ConstructorWithNullEmployeeServiceShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MedicalAidEligibilityService(null, _mockMedicalOptionRepository.Object));
    }

    [Fact]
    public void ConstructorWithNullMedicalOptionRepositoryShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MedicalAidEligibilityService(_mockEmployeeService.Object, null));
    }

    #endregion

    #region GetEligibleMedicalOptionsForEmployeeAsync Tests

    [Fact]
    public async Task GetEligibleMedicalOptionsWithValidDataShouldReturnEligibleOptions()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 1,
            NumberOfChildren = 2
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            MonthlySalary = 25000m,
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Essential Plus",
                    MedicalOptionCategoryId = 1,
                    SalaryBracketMin = 0,
                    SalaryBracketMax = 30000,
                    TotalMonthlyContributionsPrincipal = 1500,
                    TotalMonthlyContributionsAdult = 750,
                    TotalMonthlyContributionsChild = 450,
                    MonthlyRiskContributionPrincipal = 1000,
                    MonthlyRiskContributionAdult = 500,
                    MonthlyRiskContributionChild = 300,
                    MonthlyMsaContributionPrincipal = 500,
                    MonthlyMsaContributionAdult = 250,
                    MonthlyMsaContributionChild = 150
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Essential Plus", result[0].MedicalOptionName);
        Assert.Equal(1500m, result[0].EstimatedPrincipalMonthlyPremium);
        Assert.Equal(750m, result[0].EstimatedAdultMonthlyPremium);
        Assert.Equal(900m, result[0].EstimatedChildMonthlyPremium); // 450 * 2
        Assert.Equal(3150m, result[0].EstimatedTotalMonthlyPremium);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithNullEmployeeIdShouldThrowArgumentException()
    {
        // Arrange
        var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetEligibleMedicalOptionsForEmployeeAsync(null, request));
        Assert.Contains("Employee ID cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithEmptyEmployeeIdShouldThrowArgumentException()
    {
        // Arrange
        var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetEligibleMedicalOptionsForEmployeeAsync("   ", request));
        Assert.Contains("Employee ID cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithNonExistentEmployeeShouldThrowKeyNotFoundException()
    {
        // Arrange
        var employeeId = "EMP999";
        var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync((EmployeeDto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request));
        Assert.Contains($"Employee with ID {employeeId} not found", exception.Message);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsShouldFilterBySalaryBracket()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 0,
            NumberOfChildren = 0
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            MonthlySalary = 5000m // Low salary
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "High Salary Plan",
                    SalaryBracketMin = 20000, // Above employee salary
                    SalaryBracketMax = 50000,
                    TotalMonthlyContributionsPrincipal = 1500
                },
                new MedicalOption
                {
                    MedicalOptionId = 2,
                    MedicalOptionName = "Low Salary Plan",
                    SalaryBracketMin = 0,
                    SalaryBracketMax = 10000, // Within employee salary
                    TotalMonthlyContributionsPrincipal = 500
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal("Low Salary Plan", result[0].MedicalOptionName);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithNoSalaryMaxShouldIncludeUnlimitedOptions()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };
        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            MonthlySalary = 100000m // High salary
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Premium", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Unlimited Plan",
                    SalaryBracketMin = 50000,
                    SalaryBracketMax = null, // No max limit
                    TotalMonthlyContributionsPrincipal = 5000
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal("Unlimited Plan", result[0].MedicalOptionName);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsShouldExcludeOptionsExceedingSalary()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };
        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            MonthlySalary = 3000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Expensive Plan",
                    TotalMonthlyContributionsPrincipal = 5000, // Exceeds salary
                    TotalMonthlyContributionsAdult = 0,
                    TotalMonthlyContributionsChild = 0
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithMultipleCategoriesShouldReturnAllEligible()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 1,
            NumberOfChildren = 1
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            MonthlySalary = 50000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Essential Plus",
                    TotalMonthlyContributionsPrincipal = 1000,
                    TotalMonthlyContributionsAdult = 500,
                    TotalMonthlyContributionsChild = 300
                }
            }),
            CreateMedicalOptionGroup(2, "Vital", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 2,
                    MedicalOptionName = "Vital Plan",
                    TotalMonthlyContributionsPrincipal = 800,
                    TotalMonthlyContributionsAdult = 400,
                    TotalMonthlyContributionsChild = 250
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithNoAdultsShouldReturnZeroAdultPremium()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 0,
            NumberOfChildren = 0
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            MonthlySalary = 25000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Essential Plus",
                    TotalMonthlyContributionsPrincipal = 1500,
                    TotalMonthlyContributionsAdult = 750,
                    TotalMonthlyContributionsChild = 450
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal(1500m, result[0].EstimatedPrincipalMonthlyPremium);
        Assert.Null(result[0].EstimatedAdultMonthlyPremium); // Zero adults = null
        Assert.Null(result[0].EstimatedChildMonthlyPremium); // Zero children = null
        Assert.Equal(1500m, result[0].EstimatedTotalMonthlyPremium);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsWithZeroChildrenShouldReturnZeroChildPremium()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 0,
            NumberOfChildren = 0
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            MonthlySalary = 25000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Essential Plus",
                    TotalMonthlyContributionsPrincipal = 1500,
                    TotalMonthlyContributionsChild = 450
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Null(result[0].EstimatedChildMonthlyPremium);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsShouldMapEmployeeDetailsCorrectly()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 2,
            NumberOfChildren = 3
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            MonthlySalary = 30000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Essential Plus",
                    MedicalOptionCategory = new MedicalOptionCategory
                    {
                        MedicalOptionCategoryName = "Essential"
                    },
                    TotalMonthlyContributionsPrincipal = 1500
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result[0].EmployeeName);
        Assert.Equal("Doe", result[0].EmployeeSurname);
        Assert.Equal(30000m, result[0].Salary);
        Assert.Equal(1, result[0].NumberOfPrincipals);
        Assert.Equal(2, result[0].NumberOfAdults);
        Assert.Equal(3, result[0].NumberOfChildren);
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsShouldCalculateMultipleAdultsCorrectly()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 3, // Multiple adults
            NumberOfChildren = 0
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            MonthlySalary = 50000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Family Plan",
                    TotalMonthlyContributionsPrincipal = 2000,
                    TotalMonthlyContributionsAdult = 1000 // Per adult
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal(3000m, result[0].EstimatedAdultMonthlyPremium); // 1000 * 3
        Assert.Equal(5000m, result[0].EstimatedTotalMonthlyPremium); // 2000 + 3000
    }

    [Fact]
    public async Task GetEligibleMedicalOptionsShouldCalculateMultipleChildrenCorrectly()
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = 1,
            NumberOfAdults = 0,
            NumberOfChildren = 4 // Multiple children
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            MonthlySalary = 40000m
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, "Essential", new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Family Plan",
                    TotalMonthlyContributionsPrincipal = 1500,
                    TotalMonthlyContributionsChild = 400 // Per child
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal(1600m, result[0].EstimatedChildMonthlyPremium); // 400 * 4
        Assert.Equal(3100m, result[0].EstimatedTotalMonthlyPremium); // 1500 + 1600
    }

    #endregion

    #region Theory Tests - Multiple Salary Scenarios

    [Theory]
    [InlineData(5000, 1, 0, 0, 500, true)]    // Low salary, single principal, qualifies for low plan
    [InlineData(15000, 1, 1, 0, 1000, true)]  // Mid salary, principal + adult, qualifies
    [InlineData(50000, 1, 2, 3, 5000, true)]  // High salary, full family, qualifies for premium
    [InlineData(2000, 1, 0, 0, 500, false)]   // Too low salary, doesn't qualify
    [InlineData(10000, 1, 5, 5, 8000, false)] // Salary insufficient for many dependents
    public async Task GetEligibleMedicalOptionsTheoryTest(
        decimal monthlySalary, 
        int principals, 
        int adults, 
        int children, 
        decimal optionPremium,
        bool expectedToQualify)
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = principals,
            NumberOfAdults = adults,
            NumberOfChildren = children
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "Test",
            MonthlySalary = monthlySalary
        };

        var totalRequired = optionPremium + (adults * 500) + (children * 300);

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Test Plan",
                    TotalMonthlyContributionsPrincipal = optionPremium,
                    TotalMonthlyContributionsAdult = 500,
                    TotalMonthlyContributionsChild = 300
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        if (expectedToQualify)
        {
            Assert.NotEmpty(result);
        }
        else
        {
            Assert.Empty(result);
        }
    }

    #endregion

    #region Theory Tests - Dependent Count Variations

    [Theory]
    [InlineData(1, 0, 0, 1500, 1500)]    // Just principal
    [InlineData(1, 1, 0, 1500, 2250)]   // Principal + 1 adult
    [InlineData(1, 0, 2, 1500, 2400)]   // Principal + 2 children
    [InlineData(1, 2, 3, 1500, 4200)]   // Full family
    public async Task GetEligibleMedicalOptionsDependentCalculationTheory(
        int principals,
        int adults,
        int children,
        decimal principalContribution,
        decimal expectedTotal)
    {
        // Arrange
        var employeeId = "EMP001";
        var request = new RequestEligibileOptionsDto
        {
            NumberOfPrincipals = principals,
            NumberOfAdults = adults,
            NumberOfChildren = children
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "Test",
            MonthlySalary = 100000m // High enough to qualify
        };

        var medicalOptions = new List<IGrouping<int, MedicalOption>>
        {
            CreateMedicalOptionGroup(1, new[]
            {
                new MedicalOption
                {
                    MedicalOptionId = 1,
                    MedicalOptionName = "Family Plan",
                    TotalMonthlyContributionsPrincipal = principalContribution,
                    TotalMonthlyContributionsAdult = 750,
                    TotalMonthlyContributionsChild = 450
                }
            })
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);

        // Act
        var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedTotal, result[0].EstimatedTotalMonthlyPremium);
    }

    #endregion

    #region Helper Methods

    private static IGrouping<int, MedicalOption> CreateMedicalOptionGroup(int key, IEnumerable<MedicalOption> options)
    {
        return options.GroupBy(o => key).First();
    }

    #endregion
}