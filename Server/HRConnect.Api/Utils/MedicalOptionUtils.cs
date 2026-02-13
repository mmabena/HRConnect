namespace HRConnect.Api.Utils
{
  public static class MedicalOptionUtils
  {
    public static string OptionNameFormatter(string optionName)
    {
      return optionName.Replace(optionName.Last().ToString(), "").TrimEnd();
    }
  } 
}