namespace HRConnect.Tests
{
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Services;
  using Microsoft.AspNetCore.Http;
  using HRConnect.Api.Repositories;
  using OfficeOpenXml;
  using Moq;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Threading.Tasks;
  using Xunit;


  public class TaxTableUploadServiceTests
  {
    public TaxTableUploadServiceTests()
    {
      ExcelPackage.License.SetNonCommercialPersonal("Test License");
    }

    private static FormFile CreateValidExcelFile()
    {
      using var package = new ExcelPackage();
      var sheet = package.Workbook.Worksheets.Add("TaxTable");

      // Headers
      sheet.Cells[1, 1].Value = "Remuneration";
      sheet.Cells[1, 2].Value = "AnnualEquivalent";
      sheet.Cells[1, 3].Value = "TaxUnder65";
      sheet.Cells[1, 4].Value = "Tax65To74";
      sheet.Cells[1, 5].Value = "TaxOver75";

      // Valid row values
      sheet.Cells[2, 1].Value = "0-5000";
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
    public async Task UploadTaxTableAsyncValidExcelCallsRepositories()
    {
      var mockUploadRepo = new Mock<ITaxTableUploadRepository>();
      var mockDeductionRepo = new Mock<ITaxDeductionRepository>();

      mockUploadRepo.Setup(r => r.GetActiveTaxTableUploadsAsync())
          .ReturnsAsync(new List<TaxTableUpload>());

      var service = new TaxTableUploadService(mockUploadRepo.Object, mockDeductionRepo.Object);
      var file = CreateValidExcelFile();
      var taxYear = 2026;

      var result = await service.UploadTaxTableAsync(taxYear, file);

      // Verify AddTaxDeductionsAsync was called
      mockDeductionRepo.Verify(r => r.AddTaxDeductionsAsync(It.Is<List<TaxDeduction>>(list => list.Count == 1)), Times.Once);

      // Verify AddTaxTableUploadAsync was called
      mockUploadRepo.Verify(r => r.AddTaxTableUploadAsync(It.Is<TaxTableUpload>(x => x.TaxYear == taxYear)), Times.Once);

      // Verify the result
      Assert.Equal($"Tax table for the year {taxYear} uploaded successfully.", result.Message);
      Assert.Equal(new DateTime(taxYear, 3, 1), result.EffectiveFrom);
    }

    [Fact]
    public async Task UploadTaxTableAsyncInvalidFileTypeThrowsException()
    {
      var mockUploadRepo = new Mock<ITaxTableUploadRepository>();
      var mockDeductionRepo = new Mock<ITaxDeductionRepository>();

      var service = new TaxTableUploadService(mockUploadRepo.Object, mockDeductionRepo.Object);

      var stream = new MemoryStream(Encoding.UTF8.GetBytes("fake"));
      var file = new FormFile(stream, 0, stream.Length, "file", "tax.txt");

      await Assert.ThrowsAsync<ArgumentException>(() =>
          service.UploadTaxTableAsync(2026, file));
    }

    [Fact]
    public async Task UploadTaxTableAsyncSameYearThrowsException()
    {
      var existingUpload = new TaxTableUpload { TaxYear = 2026 };
      var mockUploadRepo = new Mock<ITaxTableUploadRepository>();
      var mockDeductionRepo = new Mock<ITaxDeductionRepository>();

      mockUploadRepo.Setup(r => r.GetActiveTaxTableUploadsAsync())
          .ReturnsAsync(new List<TaxTableUpload> { existingUpload });

      var service = new TaxTableUploadService(mockUploadRepo.Object, mockDeductionRepo.Object);
      var file = CreateValidExcelFile();

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

      var mockUploadRepo = new Mock<ITaxTableUploadRepository>();
      var mockDeductionRepo = new Mock<ITaxDeductionRepository>();

      // Important: return empty list to avoid ArgumentNullException
      mockUploadRepo.Setup(r => r.GetActiveTaxTableUploadsAsync())
          .ReturnsAsync(new List<TaxTableUpload>());

      var service = new TaxTableUploadService(mockUploadRepo.Object, mockDeductionRepo.Object);

      await Assert.ThrowsAsync<ArgumentException>(() =>
          service.UploadTaxTableAsync(2026, file));
    }
  }
}
