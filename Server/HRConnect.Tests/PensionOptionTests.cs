namespace HRConnect.Tests
{
  using HRConnect.Api.Utils;

  public class PensionOptionTests
  {
    [Fact]
    public void GetPensionOptionsReturnAFloat()
    {
      //Arrange
      int index = 1;

      //Act
      float result = AvailablePensionOptions.GetPensionPercentage(index);

      //Assert
      _ = Assert.IsType<float>(result);
    }

    [Fact]
    public void GetPensionPercentageInvalidIndexThrowsException()
    {
      //Arrange
      int invalidIndex = 0;

      //Act & Assert
      _ = Assert.Throws<IndexOutOfRangeException>(() => AvailablePensionOptions.GetPensionPercentage(invalidIndex));
    }
  }
}
