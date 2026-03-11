namespace HRConnect.Api.Utils.Payroll
{
  public class PayrollUtil
  {

    /// <summary>
    /// Get the current tax month 
    /// 4 -> April. The first month of our financial period
    /// 3-> March. The last month of our financial period
    /// <summary>
    public static (DateTime start, DateTime end) GetCurrectFinancialPeriod()
    {
      DateTime today = DateTime.Now;
      int startYear;
      if (today.Month >= 4)
      {
        //April -> December: Current financial year
        startYear = today.Year;
      }
      else
      {
        // Jan -> March: Previoud financial year
        startYear = today.Year - 1;
      }
      //1st of April
      DateTime start = new DateTime(startYear, 4, 1);
      //31s of March
      DateTime end = new DateTime(startYear + 1, 3, 31);
      return (start, end);
    }
  }
}