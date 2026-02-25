namespace HRConnect.Tests
{
  using System.Reflection;
  using FluentAssertions;
  using HRConnect.Api.Utils;

  public class PayrollDeductionsCalculatorTests
  {
    private readonly PayrollDeductionsCalculator _payrollDeductionsCalculator;
    public PayrollDeductionsCalculatorTests()
    {
      _payrollDeductionsCalculator = new PayrollDeductionsCalculator();
    }

    [Theory]
    [MemberData(nameof(PayrollDeductionsTestData.UifForSalaryBelowCap),
    MemberType = typeof(PayrollDeductionsTestData))]
    public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryBelowCap(decimal monthlySalary, decimal expectedAmount)
    {
      //Act and Arrange 
      var result = _payrollDeductionsCalculator.CalculateUif(monthlySalary);

      //Assert
      Assert.Equal(expectedAmount, result.employeeAmount);
    }

    [Theory]
    [MemberData(nameof(PayrollDeductionsTestData.UifAboveCapTestData),
    MemberType = typeof(PayrollDeductionsTestData))]
    public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryAboveUifCap(decimal monthlySalary, decimal expectedEmployeeAmount = 8856, decimal expectedEmployerAmount = 8856)
    {
      //Arrange and assert
      var result = _payrollDeductionsCalculator.CalculateUif(monthlySalary);

      //Assert 
      Assert.Equal(expectedEmployeeAmount, result.employeeAmount);
      Assert.Equal(expectedEmployerAmount, result.employerAmount);
    }

    [Theory]
    [InlineData(0)]
    public void CalculateDeductionsShouldReturnZeroForZeroSalary(decimal monthlySalary, decimal expectedAmount = 0)
    {
      // Arrange and Act
      var result = _payrollDeductionsCalculator.CalculateUif(monthlySalary);

      // Assert
      Assert.Equal(expectedAmount, result.employeeAmount);
      Assert.Equal(expectedAmount, result.employerAmount);
    }

    [Theory]
    [MemberData(nameof(PayrollDeductionsTestData.SdlAmountTestData),
    MemberType = typeof(PayrollDeductionsTestData))]
    public void CalculateDeductionsShouldCalculateSdlCorrectly(decimal monthlySalary, decimal expectedSdlAmount)
    {
      //Arrange
      decimal result;

      //Act
      result = _payrollDeductionsCalculator.CalculateSdlAmount(monthlySalary);

      Assert.Equal(expectedSdlAmount, result);
    }

    [Theory]
    [MemberData(nameof(PayrollDeductionsTestData.UifEmployeeAndEmployerTestData),
    MemberType = typeof(PayrollDeductionsTestData))]
    public void CalculateDeductionsShouldCalculateUifEmployeeAndEmployerCorrectly(decimal monthlySalary, decimal expectedEmployeeAmount, decimal expectedEmployerAmount)
    {
      //Arrange and Act
      var result = _payrollDeductionsCalculator.CalculateUif(monthlySalary);

      //Assert
      Assert.Equal(expectedEmployeeAmount, result.employeeAmount);
      Assert.Equal(expectedEmployerAmount, result.employerAmount);
      //Both employee and employer amount should be the same amount
      Assert.Equal(result.employeeAmount, result.employerAmount);
    }
    [Fact]
    public void CalculateDeductionsShouldThrowExceptionOnNegativeSalary()
    {
      //Arrange 
      var payrollCalculator = new PayrollDeductionsCalculator();

      Action result = () => payrollCalculator.CalculateUifEmployee(-1m);

      //Assert
      result.Should().Throw<ArgumentOutOfRangeException>()
      .WithParameterName("monthlySalary")
      .WithMessage("Monthly salary cannot be negative value*");
    }
  }
}