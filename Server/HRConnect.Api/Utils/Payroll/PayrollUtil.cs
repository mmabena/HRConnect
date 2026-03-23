namespace HRConnect.Api.Utils.Payroll
{
  using HRConnect.Api.Models.Payroll;
  using OfficeOpenXml;
  using System.IO;
  using System.Globalization;
  using System.Reflection;

  public class PayrollUtil
  {
    /// <summary>
    /// Gets and calculate the current year's financial period
    /// <returns>
    ///(<c>start</c>, <c><end/c>) represent the start and end of each financial year respectively. 
    /// Current a finacial year should start on 1st of April and on 31st March
    /// <returns>
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
        // Jan -> March: Previous financial year
        startYear = today.Year - 1;
      }
      //1st of April
      DateTime start = new DateTime(startYear, 4, 1);
      //31s of March
      DateTime end = new DateTime(startYear + 1, 3, 31);
      return (start, end);
    }

    /// <summary >
    /// Helper function to get the current payroll run number based on current date
    /// <para name="currentDate">The date used to find the desired run <param> 
    /// <summary >
    public static int SetPayrunNumber()
    {
      return ((DateTime.Now.Month + 8) % 12) + 1;
    }
    /// <summary>
    /// Utility method to get the public properties of any instance of Type t and then flatten the hierarchy 
    /// </summary>
    /// <param name="t">An instance of the PayrollRecord type</param>
    /// <returns></returns>
    private static PropertyInfo[] GetAllPublicPropertiesFromRecords(Type t)
    {
      return t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }


  }
}