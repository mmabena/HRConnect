namespace HRConnect.Tests
{
  using Moq;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Services;

  public class PayrollContributionsServiceTests
  {
    private readonly Mock<IPayrollDeductionsRepository> _payrollDeductionRepoMock;
    private readonly Mock<IPayrollDeductionsService> _payrollDeductionServiceMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly PayrollDeductionsService _payrollDeductionService;
    private readonly PayrollDeductionsCalculator _deductionsCalculator = new PayrollDeductionsCalculator();
    public PayrollContributionsServiceTests()
    {
      _payrollDeductionRepoMock = new Mock<IPayrollDeductionsRepository>();
      _payrollDeductionServiceMock = new Mock<IPayrollDeductionsService>();
      _employeeRepoMock = new Mock<IEmployeeRepository>();

      _payrollDeductionService = new PayrollDeductionsService(
        _employeeRepoMock.Object,
        _payrollDeductionRepoMock.Object
      );
    }

    [Fact]
    public async Task GetDeductionsByEmployeeIdAsyncDeductionNotFoundReturnsNull()
    {
      //Arrange
      _payrollDeductionRepoMock.Setup(p => p.GetDeductionsByEmployeeIdAsync(
        It.IsAny<int>()))
        .ReturnsAsync((PayrollDeduction)null!);

      //Act
      var result = await _payrollDeductionService.GetDeductionsByEmployeeIdAsync(1);

      //Assert
      Assert.Null(result);
    }
    [Fact]
    public async Task AddDeductionsAsyncReturnsDeductions()
    {
      //Arrange
      //Deductions depend on an Employee existing
      int employeeId = 1;
      var employee = new Employee
      {
        EmployeeId = employeeId,
        Name = "Worker",
        EmployeeCode = "WOR001",
        MonthlySalary = 8500m,
        IdNumber = "0201065054888",
        PassportNumber = ""
      };

      _employeeRepoMock.Setup(repo => repo.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);

      //Mock Setup of Employee Update As this is used by AddDeduction service method
      _employeeRepoMock.Setup(r => r.UpdateEmployeeAsync(1, It.IsAny<Employee>())).ReturnsAsync(employee);

      _payrollDeductionRepoMock.Setup(repo => repo.AddDeductionsAsync(It.IsAny<PayrollDeduction>())).ReturnsAsync((PayrollDeduction d) => d);

      //Act 
      var result = await _payrollDeductionService.AddDeductionsAsync(employeeId);

      //Assert
      Assert.NotNull(result);
    }
  }
}