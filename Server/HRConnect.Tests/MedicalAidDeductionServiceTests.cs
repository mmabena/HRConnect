namespace HRConnect.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRConnect.Api.DTOs.Employee;
using HRConnect.Api.DTOs.MedicalOption;
using HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using HRConnect.Api.Models.Payroll;
using HRConnect.Api.Models.PayrollDeduction;
using HRConnect.Api.Services;
using HRConnect.Tests.SampleData;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class MedicalAidDeductionServiceTests
{
    private readonly Mock<IMedicalAidDeductionRepository> _mockDeductionRepository;
    private readonly Mock<IMedicalOptionRepository> _mockMedicalOptionRepository;
    private readonly Mock<IEmployeeService> _mockEmployeeService;
    private readonly Mock<IPayrollRunService> _mockPayrollRunService;
    private readonly Mock<IMedicalAidEligibilityService> _mockMedicalAidEligibilityService;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IMedicalOptionService> _mockMedicalOptionService;
    private readonly MedicalAidDeductionService _service;

    public MedicalAidDeductionServiceTests()
    {
        _mockDeductionRepository = new Mock<IMedicalAidDeductionRepository>();
        _mockMedicalOptionRepository = new Mock<IMedicalOptionRepository>();
        _mockEmployeeService = new Mock<IEmployeeService>();
        _mockPayrollRunService = new Mock<IPayrollRunService>();
        _mockMedicalAidEligibilityService = new Mock<IMedicalAidEligibilityService>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockMedicalOptionService = new Mock<IMedicalOptionService>();

        _service = new MedicalAidDeductionService(
            _mockDeductionRepository.Object,
            _mockMedicalOptionRepository.Object,
            _mockEmployeeService.Object,
            _mockPayrollRunService.Object,
            _mockMedicalAidEligibilityService.Object,
            _mockServiceScopeFactory.Object);
    }

    [Fact]
    public void ConstructorShouldInitializeService()
    {
        var service = new MedicalAidDeductionService(
            _mockDeductionRepository.Object,
            _mockMedicalOptionRepository.Object,
            _mockEmployeeService.Object,
            _mockPayrollRunService.Object,
            _mockMedicalAidEligibilityService.Object,
            _mockServiceScopeFactory.Object);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetDeductionByEmployeeIdWithValidIdShouldReturnDeduction()
    {
        var employeeId = "EMP001";
        
        // Use dummy data for the deduction
        var deduction = DummyPayrollRecords.CreateMedicalAidDeduction(
            id: 1,
            payrollRunId: 1,
            employeeId: employeeId,
            isLocked: false);

        var deductions = new List<MedicalAidDeduction> { deduction };

        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(deductions);

        var result = await _service.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
        Assert.Equal("Doe", result.Surname);
        Assert.Equal(1500, result.PrincipalPremium);
    }

    [Fact]
    public async Task GetDeductionByEmployeeIdWithNoDeductionsShouldThrowKeyNotFoundException()
    {
        var employeeId = "EMP999";
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction>());

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId));
        Assert.Contains($"No medical aid deductions found for employee {employeeId}", exception.Message);
    }

    [Fact]
    public async Task AddNewDeductionWithEssentialCategoryShouldCalculateCorrectPremiums()
    {
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
            MonthlySalary = 50000,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Essential Plus",
            MedicalOptionCategoryId = 1,
            MonthlyRiskContributionPrincipal = 1000,
            MonthlyRiskContributionAdult = 500,
            MonthlyRiskContributionChild = 300,
            MonthlyMsaContributionPrincipal = 500,
            MonthlyMsaContributionAdult = 250,
            MonthlyMsaContributionChild = 150,
            TotalMonthlyContributionsPrincipal = 1500,
            TotalMonthlyContributionsAdult = 750,
            TotalMonthlyContributionsChild = 450
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
        _mockMedicalAidEligibilityService.Setup(s => s.isEligibleAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()))
            .ReturnsAsync(true);
        _mockDeductionRepository
            .Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        Assert.NotNull(result);
        Assert.Equal(1500, result.PrincipalPremium);
        Assert.Equal(750, result.SpousePremium);
        Assert.Equal(900, result.ChildPremium);
    }

    [Fact]
    public async Task AddNewDeductionWithVitalCategoryShouldExcludePrincipal()
    {
        var employeeId = "EMP001";
        var medicalOptionId = 2;

        var request = new CreateMedicalAidDeductionRequestDto
        {
            PrincipalCount = 1, // Vital plans don't have principals
            AdultCount = 0,
            ChildrenCount = 1
        };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            Surname = "Doe",
            Branch = Branch.Johannesburg,
            MonthlySalary = 30000,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = medicalOptionId,
            MedicalOptionName = "Vital Plan",
            MedicalOptionCategoryId = 2,
            MonthlyRiskContributionPrincipal = null,
            MonthlyRiskContributionAdult = 400,
            MonthlyRiskContributionChild = 250,
            TotalMonthlyContributionsPrincipal = null,
            TotalMonthlyContributionsAdult = 400,
            TotalMonthlyContributionsChild = 250
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
        _mockMedicalAidEligibilityService.Setup(s => s.isEligibleAsync(
            It.IsAny<string>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>()))
            .ReturnsAsync(true);
        _mockDeductionRepository
            .Setup(r => r.AddNewMedicalAidDeductionsAsync(It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        var result = await _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request);

        Assert.Equal(400, result.PrincipalPremium);
        Assert.Equal(0, result.SpousePremium);
        Assert.Equal(250, result.ChildPremium);
    }

    [Fact]
    public async Task AddNewDeductionWithNonExistentEmployeeShouldThrowKeyNotFoundException()
    {
        var employeeId = "EMP999";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId))
            .ReturnsAsync((EmployeeDto)null!);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains($"Employee with ID {employeeId} not found", exception.Message);
    }

    [Fact]
    public async Task AddNewDeductionWithNonPermanentEmployeeShouldThrowArgumentException()
    {
        var employeeId = "EMP001";
        var medicalOptionId = 1;
        var request = new CreateMedicalAidDeductionRequestDto { PrincipalCount = 1 };

        var employee = new EmployeeDto
        {
            EmployeeId = employeeId,
            Name = "John",
            EmploymentStatus = EmploymentStatus.Contract
        };

        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AddNewMedicalAidDeductions(employeeId, medicalOptionId, request));
        Assert.Contains("only applicable to permanent employees", exception.Message);
    }

    [Fact]
    public async Task GetAllDeductionsShouldReturnAllDeductions()
    {
        // Use dummy data for multiple deductions
        var deductions = new List<MedicalAidDeduction>
        {
            DummyPayrollRecords.CreateMedicalAidDeduction(id: 1, payrollRunId: 1, employeeId: "EMP001"),
            DummyPayrollRecords.CreateMedicalAidDeduction(id: 2, payrollRunId: 1, employeeId: "EMP002")
        };

        _mockDeductionRepository.Setup(r => r.GetAllMedicalAidDeductionsAsync())
            .ReturnsAsync(deductions);

        var result = await _service.GetAllMedicalAidDeductions();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task UpdateDeductionsByEmpIdShouldUpdateExistingDeduction()
    {
        var employeeId = "EMP001";
        var updateRequest = new UpdateMedicalAidDeductionRequestDto
        {
            MedicalOptionId = 1,
            MedicalCategoryId = 1,
            OptionName = "Essential Plus",
            OptionCategory = "Essential",
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
            MonthlySalary = 50000,
            StartDate = new DateOnly(2024, 1, 15),
            EmploymentStatus = EmploymentStatus.Permanent
        };

        var medicalOption = new MedicalOptionDto
        {
            MedicalOptionId = 1,
            MedicalOptionName = "Essential Plus",
            MedicalOptionCategoryId = 1,
            TotalMonthlyContributionsPrincipal = 1500,
            TotalMonthlyContributionsAdult = 750,
            TotalMonthlyContributionsChild = 450
        };

        var medicalOptionCategory = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = 1,
            MedicalOptionCategoryName = "Essential"
        };

        // Use dummy data for existing deduction and payroll run
        var currentRun = DummyPayrollRun.CreateActiveRun();
        var existingDeduction = DummyPayrollRecords.CreateMedicalAidDeduction(
            id: 1,
            payrollRunId: currentRun.PayrollRunId, // Use same payroll run ID as current run
            employeeId: employeeId,
            isLocked: false);

        // Mock the service scope factory
        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IPayrollRunService)))
            .Returns(_mockPayrollRunService.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmployeeService)))
            .Returns(_mockEmployeeService.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IMedicalAidDeductionRepository)))
            .Returns(_mockDeductionRepository.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IMedicalOptionService)))
            .Returns(_mockMedicalOptionService.Object);
        
        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);

        // Setup method calls
        _mockPayrollRunService.Setup(s => s.GetCurrentRunAsync()).ReturnsAsync(currentRun);
        _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(new List<MedicalAidDeduction> { existingDeduction });
        _mockMedicalOptionService.Setup(r => r.GetMedicalOptionByIdAsync(1))
            .ReturnsAsync(medicalOption);
        _mockMedicalOptionService.Setup(r => r.GetCategoryById(1))
            .ReturnsAsync(new List<MedicalOptionCategoryOnlyDto> { 
                new MedicalOptionCategoryOnlyDto { MedicalOptionCategoryName = "Essential" } 
            });
        _mockDeductionRepository.Setup(r => r.UpdateDeductionsByEmpIdAsync(employeeId, currentRun.PayrollRunId, It.IsAny<MedicalAidDeduction>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateDeductionsByEmpIdAsync(employeeId, updateRequest);

        // Assert
        Assert.NotNull(result);
        _mockDeductionRepository.Verify(r => r.UpdateDeductionsByEmpIdAsync(employeeId, currentRun.PayrollRunId, It.IsAny<MedicalAidDeduction>()), Times.Once);
    }

    [Fact]
    public async Task GetDeductionByEmployeeIdWithMultipleDeductionsShouldReturnFirstOne()
    {
        var employeeId = "EMP001";
        
        // Use dummy data to create multiple deductions for the same employee
        var deductions = new List<MedicalAidDeduction>
        {
            DummyPayrollRecords.CreateMedicalAidDeduction(id: 1, payrollRunId: 1, employeeId: employeeId),
            DummyPayrollRecords.CreateTerminatedMedicalAidDeduction(id: 2, payrollRunId: 2, employeeId: employeeId)
        };

        _mockDeductionRepository.Setup(r => r.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId))
            .ReturnsAsync(deductions);

        var result = await _service.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

        Assert.NotNull(result);
        Assert.Equal("John", result.Name);
        Assert.Equal("Doe", result.Surname);
        Assert.True(result.IsActive); // Should return the first (active) deduction
    }

    [Fact]
    public async Task GetAllDeductionsWithMixedRecordsShouldReturnCorrectCount()
    {
        // Use dummy data to create medical aid deductions for different categories
        var payrollRunId = 1;
        var medicalDeductions = DummyPayrollRecords.CreateMedicalAidDeductionsByCategory(payrollRunId);

        _mockDeductionRepository.Setup(r => r.GetAllMedicalAidDeductionsAsync())
            .ReturnsAsync(medicalDeductions);

        var result = await _service.GetAllMedicalAidDeductions();

        Assert.Equal(3, result.Count); // 3 medical aid deductions
        Assert.Contains(result, d => d.EmployeeId == "EMP001");
        Assert.Contains(result, d => d.EmployeeId == "EMP002");
        Assert.Contains(result, d => d.EmployeeId == "EMP003");
    }
}
