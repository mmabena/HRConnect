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

    public static TheoryData<decimal, decimal> SdlAmountTestData =>
        new TheoryData<decimal, decimal>
    {
     {35000m,350m},
     {1771200m,17712m},
    };

    public static TheoryData<decimal, decimal, decimal> UifEmployeeAndEmployerTestData =>
        new TheoryData<decimal, decimal, decimal>
    {
     {5000m,50m,50m},
     {1771200m,8856m,8856m},
     {20000m,200m,200m}
    };
    // [Theory]
    // [InlineData(7000m)]
    // public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryBelowCap(decimal monthlySalary)
    // { }

    [Theory]
    [InlineData(2000000)]
    public void CalculateDeductionsShouldReturnCorrectAmountsForSalaryAboveUifCap(decimal monthlySalary, decimal expectedEmployeeAmount = 8856, decimal expectedEmployerAmount = 8856)
    {
      //Arrange and assert
      var result = _payrollContributionCalculator.CalculateUif(monthlySalary);

      //Assert 
      Assert.Equal(expectedEmployeeAmount, result.employeeAmount);
      Assert.Equal(expectedEmployerAmount, result.employerAmount);
    }

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
      decimal result;

      //Act
      result = _payrollContributionCalculator.CalculateSdlAmount(monthlySalary);

      // result.Should().Be(expectedSdlAmount);
      Assert.Equal(expectedSdlAmount, result);
    }

    [Theory]
    [MemberData(nameof(UifEmployeeAndEmployerTestData))]
    public void CalculateDeductionsShouldCalculateUifEmployeeAndEmployerCorrectly(decimal monthlySalary, decimal expectedEmployeeAmount, decimal expectedEmployerAmount)
    {
      //Arrange and Act
      var result = _payrollContributionCalculator.CalculateUif(monthlySalary);

      //Assert
      Assert.Equal(expectedEmployeeAmount, result.employeeAmount);
      Assert.Equal(expectedEmployerAmount, result.employerAmount);
      //Both employee and employer amount should be the same amount
      Assert.Equal(result.employeeAmount, result.employerAmount);
    }
  }
}