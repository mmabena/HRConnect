namespace HRConnect.Tests
{
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
    public void CalculateDeductionsShouldReturnZeroForZeroSalary(decimal monthlySalary)
    {
      // Arrange
      var result;
      // Act
      result = _payrollContributionCalculator.CalculateUif(monthlySalary);

      // Assert
      result.Should().Be(0);
    }

    // [Theory]
    // public void CalculateDeductionsShouldCalculateSdlCorrectly(decimal monthlySalary) { }
    // [Theory]
    // public void CalculateDeductionsShouldCalculateUifEmployeeAndEmployerCorrectly(decimal monthlySalary) { }
  }
}