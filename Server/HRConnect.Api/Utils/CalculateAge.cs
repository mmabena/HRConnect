namespace HRConnect.Api.Utils
{
  public static class CalculateAge
  {
    ///<summary>
    ///Calculate age using date of birth
    ///</summary>
    ///<param name="DOB">Date of Birth</param>
    ///<returns>
    ///Current age
    ///</returns>
    public static int UsingDOB(DateTime DOB)
    {
      DateTime today = DateTime.Today;
      int age = today.Year - DOB.Year;

      if (today.Month < DOB.Month)
      {
        age--;
      }
      else if ((today.Month == DOB.Month) && (today.Day < DOB.Day))
      {
        age--;
      }

      return age;
    }
  }
}