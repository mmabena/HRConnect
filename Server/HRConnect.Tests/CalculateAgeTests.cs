namespace HRConnect.Tests
{
  using System.Globalization;
  using HRConnect.Api.Utils;

  public class CalculateAgeTests
  {
    [Fact]
    public void CalculateAgeUsingDOBReturnsAgeAbove18()
    {
      //Arrange
      DateTime dateOfBirth = DateTime.ParseExact("1997-08-26", "yyyy-MM-dd", CultureInfo.InvariantCulture);

      //Act
      int currentAge = CalculateAge.UsingDOB(dateOfBirth);

      //Assert
      Assert.True(currentAge >= 18);
    }

    [Fact]
    public void CalculateAgeUsingDOBReturnsZeroForToday()
    {
      //Arrange
      DateTime dateOfBirth = DateTime.Today;

      //Act
      int currentAge = CalculateAge.UsingDOB(dateOfBirth);

      //Assert
      Assert.Equal(0, currentAge);
    }

    [Fact]
    public void CalculateAgeUsingDOBReturnsCorrectAgeForPastBirthday()
    {
      //Arrange
      DateTime dateOfBirth = new(2000, 1, 1);

      //Act
      int currentAge = CalculateAge.UsingDOB(dateOfBirth);

      //Assert
      Assert.True(currentAge > 20);
    }
  }
}
