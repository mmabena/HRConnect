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

    private static PropertyInfo[] GetAllPublicPropertiesFromRecords(Type t)
    {
      return t.GetProperties(BindingFlags.Public |
                             BindingFlags.Instance |
                             BindingFlags.FlattenHierarchy);
    }
    /// <summary>
    /// Write an Excel workbook containing every record in <paramref name="run"/>.
    /// </summary>
    public static async Task<string> WriteExcelAsync(PayrollRun run)
    {
      //Enable to noncommercial lisence
      ExcelPackage.License.SetNonCommercialPersonal("YourName"); //already in program cs so probably remove?

      var reportsFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "Reports"));

      string destinationFolder = Path.Combine(reportsFolder, run.PayrollRunNumber.ToString(CultureInfo.InvariantCulture));//, ;
      Directory.CreateDirectory(destinationFolder);
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
        worksheet.Cells[1, 1].Value = "PayrollRunNumber";
        // Write header row
        for (int col = 0; col < props.Length; col++)
          worksheet.Cells[1, col + 2].Value = props[col].Name;

        // Write data rows
        int row = 2;
        foreach (var record in group)
        {
          worksheet.Cells[row, 1].Value = run.PayrollRunNumber;

          for (int col = 0; col < props.Length; col++)
          {
            var value = props[col].GetValue(record);
            worksheet.Cells[row, col + 1].Value = value;
          }
          row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
      }

      await package.SaveAsAsync(new FileInfo(filePath));
      Console.WriteLine($"Payroll Excel report generated: {filePath}");
      return filePath;
    }
  }
}