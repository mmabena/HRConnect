namespace HRConnect.Tests
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Services;
  using HRConnect.Api.Models;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using OfficeOpenXml;
  using System.IO;
  using System.Text;
  using System.Linq;
  using System.Threading.Tasks;
  using Xunit;

  public class TaxTableUploadServiceTests
  {
    public TaxTableUploadServiceTests()
    {
      ExcelPackage.License.SetNonCommercialPersonal("Test License");
    }

    private static ApplicationDBContext GetDbContext()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      return new ApplicationDBContext(options);
    }

    private static FormFile CreateValidExcelFile()
    {
      using var package = new ExcelPackage();
      var sheet = package.Workbook.Worksheets.Add("TaxTable");

      // Headers with mock excel
      sheet.Cells[1, 1].Value = "Remuneration";
      sheet.Cells[1, 2].Value = "AnnualEquivalent";
      sheet.Cells[1, 3].Value = "TaxUnder65";
      sheet.Cells[1, 4].Value = "Tax65To74";
      sheet.Cells[1, 5].Value = "TaxOver75";

      // Valid row values according to service validation
      sheet.Cells[2, 1].Value = 0 - 5000;
      sheet.Cells[2, 2].Value = 12000;
      sheet.Cells[2, 3].Value = 2000;
      sheet.Cells[2, 4].Value = 1800;
      sheet.Cells[2, 5].Value = 1500;

      var stream = new MemoryStream(package.GetAsByteArray());
      stream.Position = 0;

      return new FormFile(stream, 0, stream.Length, "file", "tax.xlsx")
      {
        Headers = new HeaderDictionary(),
        ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
      };
    }

    [Fact]
    public async Task UploadTaxTableAsyncValidExcelCreatesActiveUpload()
    {
      using var context = GetDbContext();
      var service = new TaxDeductionService(context);
      var file = CreateValidExcelFile();
      var taxYear = 2026;

      await service.UploadTaxTableAsync(taxYear, file);

      var upload = await context.TaxTableUploads.FirstOrDefaultAsync();

      Assert.NotNull(upload);
      Assert.Equal(taxYear, upload!.TaxYear);
      Assert.True(upload.IsActive);
    }

    [Fact]
    public async Task UploadTaxTableAsyncInvalidFileTypeThrowsException()
    {
      using var context = GetDbContext();
      var service = new TaxDeductionService(context);

      var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake"));
      var file = new FormFile(stream, 0, stream.Length, "file", "tax.txt");

      await Assert.ThrowsAsync<ArgumentException>(() =>
          service.UploadTaxTableAsync(2026, file));
    }

    [Fact]
    public async Task UploadTaxTableAsyncInvalidHeadersThrowsException()
    {
      using var package = new ExcelPackage();
      var sheet = package.Workbook.Worksheets.Add("BadHeaders");

      sheet.Cells[1, 1].Value = "WRONG_HEADER";

      var stream = new MemoryStream(package.GetAsByteArray());
      var file = new FormFile(stream, 0, stream.Length, "file", "tax.xlsx");

      using var context = GetDbContext();
      var service = new TaxDeductionService(context);

      await Assert.ThrowsAsync<ArgumentException>(() =>
          service.UploadTaxTableAsync(2026, file));
    }

    [Fact]
    public async Task UploadTaxTableAsyncNewUploadDeactivatesPreviousOlderYears()
    {
      using var context = GetDbContext();

      // Add older year active uploads
      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2024,
        FileName = "2024.xlsx",
        FileUrl = "2024",
        IsActive = true
      });
      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2025,
        FileName = "2025.xlsx",
        FileUrl = "2025",
        IsActive = true
      });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);
      var file = CreateValidExcelFile();

      var newTaxYear = 2026;
      await service.UploadTaxTableAsync(newTaxYear, file);

      var uploads = await context.TaxTableUploads.ToListAsync();

      // Older years will be inactive
      Assert.All(uploads.Where(x => x.TaxYear < newTaxYear), x => Assert.False(x.IsActive));

      // New upload is active
      var latest = uploads.First(x => x.TaxYear == newTaxYear);
      Assert.True(latest.IsActive);
    }


    [Fact]
    public async Task UploadTaxTableAsyncSameYearThrowsException()
    {
      using var context = GetDbContext();

      // Existing active upload for the same year
      context.TaxTableUploads.Add(new TaxTableUpload
      {
        TaxYear = 2026,
        FileName = "old.xlsx",
        FileUrl = "old",
        IsActive = true
      });

      await context.SaveChangesAsync();

      var service = new TaxDeductionService(context);
      var file = CreateValidExcelFile();

      // Service should throw because same-year upload exists
      await Assert.ThrowsAsync<ArgumentException>(() =>
          service.UploadTaxTableAsync(2026, file));
    }
  }
}
