namespace HRConnect.Tests;

using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using HRConnect.Api.Services;
using HRConnect.Api.Interfaces;
using HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using HRConnect.Api.DTOs.MedicalOption;
using HRConnect.Api.DTOs.Employee;
using HRConnect.Api.Models;
using HRConnect.Api.Models.PayrollDeduction;

public class MedicalAidDeductionServiceTests
{
    private readonly Mock<IMedicalAidDeductionRepository> _mockDeductionRepository;
    private readonly Mock<IMedicalOptionRepository> _mockMedicalOptionRepository;
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IPayrollRunService> _mockPayrollRunService;
    private readonly MedicalAidDeductionService _service;

    public MedicalAidDeductionServiceTests()
    {
        _mockDeductionRepository = new Mock<IMedicalAidDeductionRepository>();
        _mockMedicalOptionRepository = new Mock<IMedicalOptionRepository>();
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockPayrollRunService = new Mock<IPayrollRunService>();
        
        _service = new MedicalAidDeductionService(
            _mockDeductionRepository.Object,
            _mockMedicalOptionRepository.Object,
            _mockEmployeeService.Object,
            _mockPayrollRunService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ConstructorWithValidDependenciesShouldInitializeService()
    {
        // Act
        var service = new MedicalAidDeductionService(
            _mockDeductionRepository.Object,
            _mockMedicalOptionRepository.Object,
            _mockEmployeeService.Object,
            _mockPayrollRunService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void ConstructorWithNullRepositoryShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new MedicalAidDeductionService(null, _mockMedicalOptionRepository.Object, _mockEmployeeService.Object, _mockPayrollRunService.Object));
    }

    #endregion

    #region GetMedicalAidDeductionsByEmployeeIdAsync Tests

    [Fact]
    public async Task GetMedicalAidDeductionsByEmployeeIdWithValidIdShouldReturnDeduction()
    {
        // Arrange
        var employeeId = "EMP001";
        var deductions = new List<MedicalAidDeduction>
        {
            new()
            {
                MedicalAidDeductionId = 1,
                EmployeeId = employeeId,
                Name = "John",
                Surname = "Doe",
                PrincipalPremium = 1500m,
                TotalDeductionAmount = 2500m,
                IsActive = true
            }
        };

        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(deductions);

        // Act
        var result = await _service.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
        Assert.Equal("Doe", result.Surname);
        Assert.Equal(1500m, result.PrincipalPremium);
    }

    [Fact]
    public async Task GetMedicalAidDeductionsByEmployeeIdWithNoDeductionsShouldThrowKeyNotFoundException()
    {
        // Arrange
        var employeeId = "EMP999";
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId));
        Assert.Contains($"No medical aid deductions found for employee {employeeId}", exception.Message);
    }

    [Fact]
    public async Task GetMedicalAidDeductionsByEmployeeIdWithNullResultShouldThrowKeyNotFoundException()
    {
        // Arrange
        var employeeId = "EMP999";
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync((List<MedicalAidDeduction>)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId));
    }

    #endregion

    #region GetAllMedicalAidDeductions Tests

    [Fact]
    public async Task GetAllMedicalAidDeductionsShouldReturnAllDeductions()
    {
        // Arrange
        var deductions = new List<MedicalAidDeduction>
        {
            new() { MedicalAidDeductionId = 1, Name = "John" },
            new() { MedicalAidDeductionId = 2, Name = "Jane" }
        };

        _mockDeductionRepository.Setup(r => r.GetAllMedicalAidDeductionsAsync())
            .ReturnsAsync(deductions);

        // Act
        var result = await _service.GetAllMedicalAidDeductions();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllMedicalAidDeductionsWithEmptyListShouldReturnEmpty()
    {
        // Arrange
        _mockDeductionRepository.Setup(r => r.GetAllMedicalAidDeductionsAsync())
            .ReturnsAsync(new List<MedicalAidDeduction>());

        // Act
        var result = await _service.GetAllMedicalAidDeductions();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region AddNewMedicalAidDeductions Tests - Essential Category

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithEssentialCategoryShouldCalculateCorrectPremiums()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;
        
        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 1,
            ChildrenCount = 2
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 50000m,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Essential Plus",
            MedicalOptionCategoryId = 1,
            MonthlyRiskContributionPrincipal = 1000m,
            MonthlyRiskContributionAdult = 500m,
            MonthlyRiskContributionChild = 300m,
            MonthlyMsaContributionPrincipal = 500m,
            MonthlyMsaContributionAdult = 250m,
            MonthlyMsaContributionChild = 150m,
            TotalMonthlyContributionsPrincipal = 1500m,
            TotalMonthlyContributionsAdult = 750m,
            TotalMonthlyContributionsChild = 450m
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 1,
            MedicalOptionCategoryName = "Essential"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1500m, result.PrincipalPremium); // 1000 + 500
        Assert.Equal(750m, result.SpousePremium);    // 500 + 250
        Assert.Equal(900m, result.ChildPremium);     // (300 + 150) * 2 = 900
    }

    #endregion

    #region AddNewMedicalAidDeductions Tests - Vital Category

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithVitalCategoryShouldExcludePrincipal()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 2;

        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 1,
            ChildrenCount = 1
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 30000m,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Vital Plan",
            MedicalOptionCategoryId = 2,
            MonthlyRiskContributionPrincipal = null, // No principal for Vital
            MonthlyRiskContributionAdult = 400m,
            MonthlyRiskContributionChild = 250m,
            TotalMonthlyContributionsPrincipal = null,
            TotalMonthlyContributionsAdult = 400m,
            TotalMonthlyContributionsChild = 250m
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 2,
            MedicalOptionCategoryName = "Vital"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(2))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        // Assert
        Assert.Equal(0m, result.PrincipalPremium); // Principal is 0 for Vital
        Assert.Equal(400m, result.SpousePremium);
        Assert.Equal(250m, result.ChildPremium);
    }

    #endregion

    #region AddNewMedicalAidDeductions Tests - Double Category

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithDoubleCategoryShouldCombineMsaAndRisk()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 3;

        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 2,
            ChildrenCount = 0
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 40000m,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Double Plan",
            MedicalOptionCategoryId = 3,
            MonthlyMsaContributionAdult = 300m,
            MonthlyRiskContributionAdult = 400m,
            MonthlyMsaContributionChild = 200m,
            MonthlyRiskContributionChild = 250m,
            TotalMonthlyContributionsAdult = 700m, // 300 + 400
            TotalMonthlyContributionsChild = 450m  // 200 + 250
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 3,
            MedicalOptionCategoryName = "Double"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(3))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        // Assert
        Assert.Equal(0m, result.PrincipalPremium); // No principal for Double
        Assert.Equal(1400m, result.SpousePremium); // 700 * 2 adults
    }

    #endregion

    #region AddNewMedicalAidDeductions Tests - Validation Errors

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithNonExistentEmployeeShouldThrowKeyNotFoundException()
    {
        // Arrange
        var employeeId = "EMP999";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId))
            .ReturnsAsync((EmployeeDto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains($"Employee with ID {employeeId} not found", exception.Message);
    }

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithNonPermanentEmployeeShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            EmploymentStatus = EmploymentStatus.Contract // Not permanent
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains("only applicable to permanent employees", exception.Message);
    }

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithExistingActiveDeductionShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var existingDeductions = new List<MedicalAidDeduction>
        {
            new() { MedicalAidDeductionId = 1, IsActive = true }
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(existingDeductions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains("existing medical aid deduction", exception.Message);
    }

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithNonExistentMedicalOptionShouldThrowKeyNotFoundException()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 999;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            EmploymentStatus = EmploymentStatus.Permanent
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync((MedicalOptionDto)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
    }

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithSalaryLessThanPremiumShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 1,
            ChildrenCount = 2
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 1000m, // Very low salary
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Expensive Plan",
            MedicalOptionCategoryId = 1,
            TotalMonthlyContributionsPrincipal = 1500m,
            TotalMonthlyContributionsAdult = 750m,
            TotalMonthlyContributionsChild = 450m
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 1,
            MedicalOptionCategoryName = "Essential"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains("exceeds monthly salary", exception.Message);
    }

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithInvalidCategoryShouldThrowArgumentException()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionCategoryId = 99
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 99,
            MedicalOptionCategoryName = "InvalidCategory"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(99))
            .ReturnsAsync(category);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains("Invalid medical option category", exception.Message);
    }

    #endregion

    #region AddNewMedicalAidDeductions Tests - Network Choice Logic

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithNetworkChoiceVariant1To3ShouldApplyFreeChild2()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;

        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 0,
            ChildrenCount = 3 // Multiple children
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 50000m,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Network Choice 2", // Variant 2 - child2+ is free
            MedicalOptionCategoryId = 1,
            MonthlyRiskContributionPrincipal = 1000m,
            MonthlyRiskContributionAdult = 500m,
            MonthlyRiskContributionChild = 300m,
            MonthlyRiskContributionChild2 = 0, // Free for variants 1-3
            TotalMonthlyContributionsPrincipal = 1000m,
            TotalMonthlyContributionsAdult = 500m,
            TotalMonthlyContributionsChild = 300m
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 1,
            MedicalOptionCategoryName = "Network Choice"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        // Assert
        Assert.NotNull(result);
        // For Network Choice variants 1-3, child2+ is free so all children pay child1 rate
    }

    [Fact]
    public async Task AddNewMedicalAidDeductionsWithNetworkChoiceVariant4PlusShouldChargeChild2()
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;

        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 0,
            ChildrenCount = 3
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 50000m,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Network Choice 5", // Variant 5 - child2+ is charged
            MedicalOptionCategoryId = 1,
            MonthlyRiskContributionPrincipal = 1000m,
            MonthlyRiskContributionChild = 300m,
            MonthlyRiskContributionChild2 = 200m, // Charged for variants 4+
            TotalMonthlyContributionsPrincipal = 1000m,
            TotalMonthlyContributionsChild = 300m
        };

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 1,
            MedicalOptionCategoryName = "Network Choice"
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);
    #endregion

    #region Theory Tests - Premium Calculation Scenarios

    [Theory]
    [InlineData("Essential", 1000, 500, 300, 1, 0, 0, 1000, 0, 0, 1000)]      // Essential: Principal only
    [InlineData("Essential", 1000, 500, 300, 1, 1, 0, 1000, 500, 0, 1500)]    // Essential: Principal + Adult
    [InlineData("Essential", 1000, 500, 300, 1, 0, 2, 1000, 0, 600, 1600)]    // Essential: Principal + 2 Children
    [InlineData("Essential", 1000, 500, 300, 1, 2, 3, 1000, 1000, 900, 2900)] // Essential: Full family
    [InlineData("Vital", 0, 400, 250, 1, 0, 0, 0, 0, 0, 0)]                  // Vital: No principal
    [InlineData("Vital", 0, 400, 250, 1, 1, 0, 0, 400, 0, 400)]              // Vital: 1 Adult
    [InlineData("Double", 0, 700, 450, 1, 2, 0, 0, 1400, 0, 1400)]           // Double: 2 Adults, no principal
    public async Task AddNewMedicalAidDeductionsTheoryTest(
        string categoryName,
        decimal riskPrincipal,
        decimal riskAdult,
        decimal riskChild,
        int principalCount,
        int adultCount,
        int childrenCount,
        decimal expectedPrincipalPremium,
        decimal expectedAdultPremium,
        decimal expectedChildPremium,
        decimal expectedTotal)
    {
        // Arrange
        var employeeId = "EMP001";
        var medicalOptionId = 1;

        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = principalCount,
            AdultCount = adultCount,
            ChildrenCount = childrenCount
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 100000m,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = $"{categoryName} Plan",
            MedicalOptionCategoryId = 1,
            MonthlyRiskContributionPrincipal = riskPrincipal > 0 ? (decimal?)riskPrincipal : null,
            MonthlyRiskContributionAdult = riskAdult,
            MonthlyRiskContributionChild = riskChild,
            TotalMonthlyContributionsPrincipal = riskPrincipal > 0 ? (decimal?)riskPrincipal : null,
            TotalMonthlyContributionsAdult = riskAdult,
            TotalMonthlyContributionsChild = riskChild
        };

        // Adjust for Essential category (MSA + Risk)
        if (categoryName == "Essential")
        {
            medicalOption.MonthlyMsaContributionPrincipal = 500;
            medicalOption.MonthlyMsaContributionAdult = 250;
            medicalOption.MonthlyMsaContributionChild = 150;
            medicalOption.TotalMonthlyContributionsPrincipal = riskPrincipal + 500;
            medicalOption.TotalMonthlyContributionsAdult = riskAdult + 250;
            medicalOption.TotalMonthlyContributionsChild = riskChild + 150;
        }

        var category = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 1,
            MedicalOptionCategoryName = categoryName
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());
        _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);
        _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPrincipalPremium, result.PrincipalPremium);
        Assert.Equal(expectedAdultPremium, result.SpousePremium);
        Assert.Equal(expectedChildPremium, result.ChildPremium);
        Assert.Equal(expectedTotal, result.TotalDeductionAmount);
    }

    #endregion

    #region Theory Tests - Validation Scenarios

    [Theory]
    [InlineData("EMP001", 50000, true, "Permanent")]   // Valid case
    [InlineData("EMP001", 1000, false, "Permanent")]   // Salary too low
    [InlineData("EMP001", 50000, true, "Contract")]  // Non-permanent
    [InlineData("EMP999", 50000, true, "Permanent")]  // Non-existent employee
    public async Task AddNewMedicalAidDeductionsValidationTheory(
        string employeeId,
        decimal monthlySalary,
        bool medicalOptionExists,
        string employmentStatus)
    {
        // Arrange
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1,
            AdultCount = 0,
            ChildrenCount = 0
        };

        EmployeeDto employee = null;
        if (employeeId == "EMP001")
        {
            employee = new EmployeeDto
            {
                EmployeeId = employeeId,
                Name = "John",
                MonthlySalary = monthlySalary,
                EmploymentStatus = employmentStatus == "Permanent" ? EmploymentStatus.Permanent : EmploymentStatus.Contract
            };
        }

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);

        if (employee != null && employmentStatus == "Permanent")
        {
            _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
                .ReturnsAsync(new List<MedicalAidDeduction>());

            if (medicalOptionExists)
            {
                var medicalOption = new MedicalOptionDto
                {
                    MedicalOptionId = medicalOptionId,
                    MedicalOptionName = "Test Plan",
                    TotalMonthlyContributionsPrincipal = 1500
                };
                var category = new MedicalOptionCategory
                {
                    MedicalOptionCategoryId = 1,
                    MedicalOptionCategoryName = "Essential"
                };
                _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
                    .ReturnsAsync(medicalOption);
                _mockMedicalOptionRepository.Setup(r => r.GetCategoryByIdAsync(1))
                    .ReturnsAsync(category);
                _mockDeductionRepository.Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
                    .Returns(Task.CompletedTask);
            }
            else
            {
                _mockMedicalOptionRepository.Setup(r => r.GetMedicalOptionByIdAsync(medicalOptionId))
                    .ReturnsAsync((MedicalOptionDto)null);
            }
        }

        // Act & Assert
        if (employeeId == "EMP999")
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        }
        else if (employmentStatus != "Permanent")
        {
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        }
        else if (!medicalOptionExists)
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        }
        else if (monthlySalary < 1500)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        }
        else
        {
            var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);
            Assert.NotNull(result);
        }
    }

    #endregion

    [Fact]
    public async Task UpdateDeductionByEmpIdShouldThrowNotImplementedException()
    {
        // Arrange
        var employeeId = "EMP001";

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(
            () => _service.UpdateDeductionByEmpId(employeeId));
    }

    #endregion
}