namespace HRConnect.Tests
{
  using System.Net.NetworkInformation;
  using System.Reflection;
  using HRConnect.Api.Utils;
  public class PayrollContributionCalculatorTests
  {
    private readonly PayrollContributionCalculator _payrollContributionCalculator;
    public PayrollContributionCalculatorTests()
    {
      _payrollContributionCalculator = new PayrollContributionCalculator();
    }
    // [Theory]
    // public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryBelowCap(decimal monthlySalary)
    // { }

    // [Theory]
    // public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryAboveUifCap(decimal monthlySalary) { }

    [Theory]
    [InlineData(0)]
    public void CalculateDeductionsShouldReturnZeroForZeroSalary(decimal monthlySalary, decimal expectedAmount = 0)
    {
      // Arrange and Act
      var result = _payrollContributionCalculator.CalculateUif(monthlySalary);

      // Assert
      Assert.Equal(expectedAmount, result.employeeAmount);
      Assert.Equal(expectedAmount, result.employerAmount);
    }

    [Theory]
    [MemberData(nameof(SdlAmountTestData))]
    public void CalculateDeductionsShouldCalculateSdlCorrectly(decimal monthlySalary, decimal expectedSdlAmount)
    {
      //Arrange
      decimal testResult;

      //Act
      testResult = _payrollContributionCalculator.CalculateSdlAmount(monthlySalary);

      testResult.Should().Be(expectedSdlAmount);
    }

    public static TheoryData<decimal, decimal> SdlAmountTestData =>
        new TheoryData<decimal, decimal>
    {
     {35000m,350m},
     {1771200m,17712m},
     {0m,0m}
    };
    // [Theory]
    // public void CalculateDeductionsShouldCalculateUifEmployeeAndEmployerCorrectly(decimal monthlySalary) { }
  }
}