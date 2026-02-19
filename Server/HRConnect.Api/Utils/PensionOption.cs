namespace HRConnect.Api.Utils
{
  public static class PensionOption
  {
    private static readonly float[] _options = [0.025f, 0.05f, 0.075f, 0.1f, 0.125f, 0.15f];

    public static float GetPensionPercentage(int selectedPercentage)
    {
      return _options[selectedPercentage - 1];
    }
  }
}