namespace HRConnect.Tests
{
  using System.Globalization;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Services;

  public class PensionProjectionServiceTests
  {
    private readonly PensionProjectionService _pensionProjectionService;

    public PensionProjectionServiceTests()
    {
      _pensionProjectionService = new PensionProjectionService();
    }

    [Fact]
    public void PensionProjectionReturnsPensionProjectionResultDto()
    {
      //Arrange
      PensionProjectionRequestDto employeeInput = new()
      {
        SelectedPensionPercentage = 3,
        DOB = DateTime.Parse("1997-08-26", CultureInfo.InvariantCulture),
        EmploymentEmploymentStatus = "Permanent",
        Salary = 30000.00M,
        VoluntaryContribution = 5000.00M,
        VoluntaryContributionFrequency = ContributionFrequency.Permanent
      };

      //Act
      PensionProjectionResultDto result = _pensionProjectionService.ProjectPension(employeeInput);

      //Assert
      Assert.NotNull(result);
      _ = Assert.IsType<PensionProjectionResultDto>(result);

      Assert.True(result.CurrentAge >= 18);
      Assert.True(result.YearsUntilRetirement > 0);
      Assert.True(result.TotalProjectedSavings >= 3000.00M);

      Assert.NotNull(result.MonthlyContribution);
      Assert.NotNull(result.MonthlyVoluntaryContribution);

      Assert.Equal(string.Empty, result.WarningMessage);
    }
  }
}
