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
      if (t != typeof(PayrollRecord))
        return [];
      return t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
    }

    /// <summary>
    /// Write an Excel workbook containing every record in <paramref name="run"/>.
    /// Each payroll record type is given it's own worksheet within the spreadsheet 
    /// </summary>
    public static async Task WriteExcelAsync(PayrollRun run)
    {
      ExcelPackage.License.SetNonCommercialPersonal("YourName"); //already in program cs so probably remove?
      try
      {

        var reportsFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "Reports"));

        string destinationFolder = Path.Combine(reportsFolder, run.PayrollRunNumber.ToString(CultureInfo.InvariantCulture));//, ;
        _ = Directory.CreateDirectory(destinationFolder);
        string filePath = Path.Combine(destinationFolder, $"PayrollRun_{run.PayrollRunNumber}.xlsx");

        using var package = new ExcelPackage();

        // Group records by concrete type
        // (PensionDeductions, MedicalAidDeductions)
        var recordsByType = run.Records.GroupBy(r => r.GetType().Name);

        foreach (var group in recordsByType)
        {
          string sheetName = group.Key;
          var worksheet = package.Workbook.Worksheets.Add(sheetName);

          // Get all public properties for this type
          var propsList = GetAllPublicPropertiesFromRecords(group.First().GetType())
          .Where(p => p.Name != "PayrollRun")
          .ToList();
          // make sure the shadow properties are copied too
          propsList.Insert(1, typeof(PayrollRecord).GetProperty("EmployeeId",
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)!);

          var props = propsList.ToArray();
          worksheet.Cells[1, 1].Value = "Payroll Run Number";
          // Write headings into the top row
          for (int col = 0; col < props.Length; col++)
            worksheet.Cells[1, col + 2].Value = props[col].Name;

          // Write data into the excel rows
          int row = 2;
          foreach (var record in group)
          {
            worksheet.Cells[row, 1].Value = run.PayrollRunNumber;
            Console.WriteLine($"?????????????????????? PAYROLL RUN NUMBER {run.PayrollRunNumber}");

            for (int col = 0; col < props.Length; col++)
            {
              var value = props[col].GetValue(record);
              worksheet.Cells[row, col + 2].Value = value;
            }
            row++;
          }
          worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
        await package.SaveAsAsync(new FileInfo(filePath));
        Console.WriteLine($"Payroll Excel report generated: {filePath}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"FAILED TO WRITE TO PATH DIRECTORY \n{ex}");
      }
    }
  }
}