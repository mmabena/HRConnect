namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using OfficeOpenXml;
  using System.IO;
  using System.Globalization;
  using System.Reflection;
  using HRConnect.Api.Models.Payroll;
  public class ReportsService : IReportsService
  {

    private readonly IWebHostEnvironment _env;
    public ReportsService(IWebHostEnvironment env)
    {
      _env = env;
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
    /// <summary>
    /// Write an Excel workbook containing every record in <paramref name="run"/>.
    /// Each payroll record type is given it's own worksheet within the spreadsheet 
    /// </summary>
    ///<remarks>Please ensure your DateTime type are assigned a value if ever marked as
    ///nullable</remarks>
    public async Task WriteExcelAsync(PayrollRun run)
    {
      ExcelPackage.License.SetNonCommercialPersonal("YourName"); //already in program cs so probably remove?

      try
      {
        string reportsFolder = Path.Combine(_env.ContentRootPath, "Reports");

        string destinationFolder = Path.Combine(reportsFolder, run.PayrollRunNumber.ToString(CultureInfo.InvariantCulture));//, ;
        _ = Directory.CreateDirectory(destinationFolder);
        string filePath = Path.Combine(destinationFolder, $"PayrollRun_{run.PayrollRunNumber}.xlsx");

        using var package = new ExcelPackage();

        // Group records by concrete type
        // (PensionDeductions, MedicalAidDeductions)
        var recordsByType = run.Records.GroupBy(r => r.GetType().Name);

        foreach (var group in recordsByType)
        {
          Console.Write($"%%%%%%RECORDS FOUND IN RUNS {group.GetType().Name}");
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

            for (int col = 0; col < props.Length; col++)
            {
              var value = props[col].GetValue(record);
              var cell = worksheet.Cells[row, col + 2];
              //For DateTime type accordingly 
              if (value is DateTime dt)
              {
                value = dt.ToLocalTime();
                cell.Style.Numberformat.Format = "yyyy-mm-dd HH:mm:ss";
              }
              if (value is DateOnly dateOnly)
              {
                value = dateOnly;
                cell.Style.Numberformat.Format = "yyyy-mm-dd";
              }
              cell.Value = value;
            }
            row++;
          }
          worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        await package.SaveAsAsync(new FileInfo(filePath));
        Console.WriteLine($"===============>Payroll Excel report generated: {filePath}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"FAILED TO WRITE TO PATH DIRECTORY \n{ex}");
      }
    }
  }
}