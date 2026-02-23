namespace HRConnect.Tests
{
  using HRConnect.Api.Utils;

  public class StatutoryContributionCalculatorTests
  {
    private readonly StatutoryContributionCalculator _statutoryContributionsCalculator;
    public StatutoryContributionCalculatorTests()
    {
      _statutoryContributionsCalculator = new StatutoryContributionCalculator();
    }

    [Theory]
    [MemberData(nameof(StatutoryContributionTestData.UifForSalaryBelowCap),
    MemberType = typeof(StatutoryContributionTestData))]
    public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryBelowCap(decimal monthlySalary, decimal expectedAmount)
    {
      //Act and Arrange 
      var result = _statutoryContributionsCalculator.CalculateUif(monthlySalary);

      //Assert
      Assert.Equal(expectedAmount, result.employeeAmount);
    }

    [Theory]
    [MemberData(nameof(StatutoryContributionTestData.UifAboveCapTestData),
    MemberType = typeof(StatutoryContributionTestData))]
    public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryAboveUifCap(decimal monthlySalary, decimal expectedEmployeeAmount = 8856, decimal expectedEmployerAmount = 8856)
    {
      //Arrange and assert
      var result = _statutoryContributionsCalculator.CalculateUif(monthlySalary);

      //Assert 
      Assert.Equal(expectedEmployeeAmount, result.employeeAmount);
      Assert.Equal(expectedEmployerAmount, result.employerAmount);
    }

    [Theory]
    [InlineData(0)]
    public void CalculateDeductionsShouldReturnZeroForZeroSalary(decimal monthlySalary, decimal expectedAmount = 0)
    {
      // Arrange and Act
      var result = _statutoryContributionsCalculator.CalculateUif(monthlySalary);

      // Assert
      Assert.Equal(expectedAmount, result.employeeAmount);
      Assert.Equal(expectedAmount, result.employerAmount);
    }

    [Theory]
    [MemberData(nameof(StatutoryContributionTestData.SdlAmountTestData),
    MemberType = typeof(StatutoryContributionTestData))]
    public void CalculateDeductionsShouldCalculateSdlCorrectly(decimal monthlySalary, decimal expectedSdlAmount)
    {
      //Arrange
      decimal result;

      //Act
      result = _statutoryContributionsCalculator.CalculateSdlAmount(monthlySalary);

      Assert.Equal(expectedSdlAmount, result);
    }

    [Theory]
    [MemberData(nameof(StatutoryContributionTestData.UifEmployeeAndEmployerTestData),
    MemberType = typeof(StatutoryContributionTestData))]
    public void CalculateDeductionsShouldCalculateUifEmployeeAndEmployerCorrectly(decimal monthlySalary, decimal expectedEmployeeAmount, decimal expectedEmployerAmount)
    {
      //Arrange and Act
      var result = _statutoryContributionsCalculator.CalculateUif(monthlySalary);

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
      var payrollCalculator = new StatutoryContributionCalculator();

      Action result = () => payrollCalculator.CalculateUifEmployee(-1m);

      //Assert
      // result.Should().Throw<ArgumentOutOfRangeException>()
      // .WithParameterName("monthlySalary")
      // .WithMessage("Monthly salary cannot be negative value*");
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() => result());

      Assert.Equal("monthlySalary", exception.ParamName);
      Assert.StartsWith("Monthly salary cannot be negative value", exception.Message);
    }
  }
}