namespace HRConnect.Tests
{
  using Moq;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Services;
  using System.Runtime.CompilerServices;
  using System.Reflection;
  using System.Security.Cryptography.X509Certificates;
  using Microsoft.VisualBasic;
  using Microsoft.AspNetCore.Mvc;
  using System.Reflection.Metadata.Ecma335;
  using System.Data.Common;
  using System.Net.Cache;

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
      //Deductions depend on employee existing
      int employeeId = 1;
      var employee = new Employee
      {
        EmployeeId = employeeId,
        Name = "Employee Name",
        EmployeeCode = "EMP001",
        MonthlySalary = 5000m,
        IdNumber = "1234567891013",
        PassportNumber = ""
      };
      _employeeRepoMock.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
      .ReturnsAsync(employee);
      var emp = await _employeeRepoMock.Object.GetEmployeeByIdAsync(1);
      Assert.NotNull(emp);
      _employeeRepoMock
    .Setup(r => r.UpdateEmployeeAsync(1, It.IsAny<Employee>()))
    .ReturnsAsync(employee);

      var deduction = new PayrollDeduction
      {
        EmployeeId = employeeId,
        MonthlySalary = employee.MonthlySalary,
        SdlAmount = employee.MonthlySalary * 0.01m,
        UifEmployeeAmount = employee.MonthlySalary * 0.01m,
        UifEmployerAmount = employee.MonthlySalary * 0.01m,
      };

      _payrollDeductionRepoMock.Setup(repo => repo.GetDeductionsByEmployeeIdAsync(employeeId))
      .ReturnsAsync((PayrollDeduction d) => d);
      //Act 
      var result = await _payrollDeductionService.AddDeductionsAsync(employeeId);

      //Assert
      // var okResult = Assert.IsType<OkObjectResult>(result);
      Assert.NotNull(result);
    }
  }
}