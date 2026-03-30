namespace HRConnect.Tests
{
  using Moq;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Services;

  public class StatutoryContributionServiceTests
  {
    private readonly Mock<IStatutoryContributionRepository> _statutoryContributionRepoMock;
    private readonly Mock<IStatutoryContributionService> _statutoryContributionServiceMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly StatutoryContributionService _statutoryContributionService;
    private readonly StatutoryContributionsCalculator _deductionsCalculator = new StatutoryContributionsCalculator();
    public StatutoryContributionServiceTests()
    {
      _statutoryContributionRepoMock = new Mock<IStatutoryContributionRepository>();
      _statutoryContributionServiceMock = new Mock<IStatutoryContributionService>();
      _employeeRepoMock = new Mock<IEmployeeRepository>();

      _statutoryContributionService = new StatutoryContributionService(
        _employeeRepoMock.Object,
        _statutoryContributionRepoMock.Object
      );
    }

    [Fact]
    public async Task GetDeductionsByEmployeeIdAsyncDeductionNotFoundReturnsNull()
    {
      //Arrange
      _statutoryContributionRepoMock.Setup(p => p.GetDeductionsByEmployeeIdAsync(
        It.IsAny<string>()))
        .ReturnsAsync((StatutoryContribution)null!);

      //Act
      var result = await _statutoryContributionService.GetDeductionsByEmployeeIdAsync("WOR001");

      //Assert
      Assert.Null(result);
    }
    [Fact]
    public async Task AddDeductionsAsyncReturnsDeductions()
    {
      //Arrange
      //Deductions depend on an Employee existing
      string employeeId = "WOR001";
      var employee = new Employee
      {
        EmployeeId = employeeId,
        Name = "Worker",
        MonthlySalary = 8500m,
        IdNumber = "0201065054888",
        PassportNumber = ""
      };

      _employeeRepoMock.Setup(repo => repo.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);

      //Mock Setup of Employee Update As this is used by AddDeduction service method
      // _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(1, It.IsAny<Employee>())).ReturnsAsync(employee);

      _statutoryContributionRepoMock.Setup(repo => repo.AddDeductionsAsync(It.IsAny<StatutoryContribution>())).ReturnsAsync((StatutoryContribution d) => d);

      //Act 
      var result = await _statutoryContributionService.AddDeductionsAsync(employeeId);

      //Assert
      Assert.NotNull(result);
    }
  }
}