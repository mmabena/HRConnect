using HRConnect.Api.DTOs;
using HRConnect.Api.DTOs.Employee;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models;
using HRConnect.Api.Services;
using Moq;

namespace HRConnect.Tests
{


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
  
    [Fact]
    public void ConstructorShouldInitializeService()
    {
      var service = new MedicalAidEligibilityService(_mockEmployeeService.Object, _mockMedicalOptionRepository.Object);
      Assert.NotNull(service);
    }
  
    [Fact]
    public async Task GetEligibleOptionsWithValidDataShouldReturnCorrectPremiums()
    {
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
        MonthlySalary = 25000,
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
            TotalMonthlyContributionsChild = 450
          }
        })
      };
  
      _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
      _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);
  
      var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);
  
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Essential Plus", result[0].MedicalOptionName);
      Assert.Equal(1500, result[0].EstimatedPrincipalMonthlyPremium);
      Assert.Equal(750, result[0].EstimatedAdultMonthlyPremium);
      Assert.Equal(900, result[0].EstimatedChildMonthlyPremium);
      Assert.Equal(3150, result[0].EstimatedTotalMonthlyPremium);
    }
  
    [Fact]
    public async Task GetEligibleOptionsWithNonExistentEmployeeShouldThrowKeyNotFoundException()
    {
      var employeeId = "EMP999";
      var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };
  
      _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync((EmployeeDto)null);
  
      await Assert.ThrowsAsync<KeyNotFoundException>(
        () => _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request));
    }
  
    [Fact]
    public async Task GetEligibleOptionsWithNonPermanentEmployeeShouldThrowArgumentException()
    {
      var employeeId = "EMP001";
      var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };
  
      var employee = new EmployeeDto
      {
        EmployeeId = employeeId,
        EmploymentStatus = EmploymentStatus.Contract
      };
  
      _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
      _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync())
        .ReturnsAsync(new List<IGrouping<int, MedicalOption>>());
  
      var exception = await Assert.ThrowsAsync<ArgumentException>(
        () => _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request));
      Assert.Contains("only available to permanent employees", exception.Message);
    }
  
    [Fact]
    public async Task GetEligibleOptionsWithSalaryTooLowShouldReturnEmptyList()
    {
      var employeeId = "EMP001";
      var request = new RequestEligibileOptionsDto { NumberOfPrincipals = 1 };
  
      var employee = new EmployeeDto
      {
        EmployeeId = employeeId,
        MonthlySalary = 1000,
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
            SalaryBracketMin = 5000,
            SalaryBracketMax = 30000,
            TotalMonthlyContributionsPrincipal = 1500
          }
        })
      };
  
      _mockEmployeeService.Setup(s => s.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);
      _mockMedicalOptionRepository.Setup(r => r.GetGroupedMedicalOptionsAsync()).ReturnsAsync(medicalOptions);
  
      var result = await _service.GetEligibleMedicalOptionsForEmployeeAsync(employeeId, request);
  
      Assert.Empty(result);
    }
  
    private static TestMedicalOptionGroup CreateMedicalOptionGroup(int categoryId, string categoryName, MedicalOption[] options)
    {
      return new TestMedicalOptionGroup(categoryId, categoryName, options);
    }
  
    private sealed class TestMedicalOptionGroup : IGrouping<int, MedicalOption>
    {
      private readonly List<MedicalOption> _options;
  
      public TestMedicalOptionGroup(int key, string categoryName, IEnumerable<MedicalOption> options)
      {
        Key = key;
        _options = options.ToList();
      }
  
      public int Key { get; }
  
      public IEnumerator<MedicalOption> GetEnumerator() => _options.GetEnumerator();
  
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
  }
}