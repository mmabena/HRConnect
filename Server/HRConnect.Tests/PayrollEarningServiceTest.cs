namespace HRConnect.Tests
{
  using HRConnect.Api.DTOs.Payroll.Earning;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll.Earning;
  using HRConnect.Api.Services;
  using HRConnect.Api.Utils;
  using Moq;
  using Quartz.Impl.Triggers;

  public class PayrollEarningServiceTest
  {
    private readonly PayrollEarningService _payrollEarningService;
    private readonly Mock<IPayrollEarningRepository> _payrollEarningRepositoryMock;

    public PayrollEarningServiceTest()
    {
      _payrollEarningRepositoryMock = new Mock<IPayrollEarningRepository>();
      _payrollEarningService = new PayrollEarningService(_payrollEarningRepositoryMock.Object);
    }

    [Fact]
    public async Task AddPayrollEarningAsyncReturnsCreatedPayrollEarning()
    {
      // Arrange
      PayrollEarningAddDto payrollEarningAddDto = new()
      {
        ShortDescription = "Test Earning",
        LongDescription = "This is a test earning for unit testing.",
        Taxable = true,
        TaxCode = 3601,
        TaxPercentage = 100m,
        OvertimeHourMultiplier = 1.5m,
        CanProRata = false,
        IsOnGoing = false,
      };

      List<string> existingPayrollEarningIds =
      [
        "PRE001"
      ];

      string generatedPayrollEarningId = GenerateUnqiueCode.GenerateStringCode("PRE", existingPayrollEarningIds);

      PayrollEarning payrollEarning = new()
      {
        PayrollEarningId = generatedPayrollEarningId,
        ShortDescription = payrollEarningAddDto.ShortDescription,
        LongDescription = payrollEarningAddDto.LongDescription,
        Taxable = payrollEarningAddDto.Taxable,
        TaxCode = payrollEarningAddDto.TaxCode,
        TaxPercentage = payrollEarningAddDto.TaxPercentage,
        OvertimeHourMultiplier = payrollEarningAddDto.OvertimeHourMultiplier,
        CanProRata = payrollEarningAddDto.CanProRata,
        IsOnGoing = payrollEarningAddDto.IsOnGoing,
      };

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetAllPayrollEarningIdsAsync("PRE"))
        .ReturnsAsync(existingPayrollEarningIds);

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.CheckIfDescriptionsExists(payrollEarningAddDto.ShortDescription, payrollEarningAddDto.LongDescription))
        .ReturnsAsync(false);

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.AddAsync(It.IsAny<PayrollEarning>()))
        .ReturnsAsync(payrollEarning);


      // Act
      PayrollEarningDto result = await _payrollEarningService.AddPayrollEarningAsync(payrollEarningAddDto);

      // Assert
      _ = Assert.IsType<PayrollEarningDto>(result);
      Assert.Equal(payrollEarning.PayrollEarningId, result.PayrollEarningId);
      Assert.Equal(payrollEarning.ShortDescription, result.ShortDescription);
      Assert.Equal(payrollEarning.LongDescription, result.LongDescription);
      Assert.Equal(payrollEarning.Taxable, result.Taxable);
      Assert.Equal(payrollEarning.TaxCode, result.TaxCode);
      Assert.Equal(payrollEarning.TaxPercentage, result.TaxPercentage);
      Assert.Equal(payrollEarning.OvertimeHourMultiplier, result.OvertimeHourMultiplier);
      Assert.Equal(payrollEarning.CanProRata, result.CanProRata);
      Assert.Equal(payrollEarning.IsOnGoing, result.IsOnGoing);
    }

    [Fact]
    public async Task GetAllPayrollEarningsAsyncReturnsListOfPayrollEarnings()
    {
      // Arrange
      List<PayrollEarning> payrollEarnings = new()
      {
        new PayrollEarning
        {
          PayrollEarningId = "PRE001",
          ShortDescription = "Test Earning 1",
          LongDescription = "This is the first test earning for unit testing.",
          Taxable = true,
          TaxCode = 3601,
          TaxPercentage = 100m,
          OvertimeHourMultiplier = 1.5m,
          CanProRata = false,
          IsOnGoing = false,
        },
        new PayrollEarning
        {
          PayrollEarningId = "PRE002",
          ShortDescription = "Test Earning 2",
          LongDescription = "This is the second test earning for unit testing.",
          Taxable = false,
          TaxCode = 3602,
          TaxPercentage = 0m,
          OvertimeHourMultiplier = 1m,
          CanProRata = true,
          IsOnGoing = true,
        }
      };
      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetAllAsync())
        .ReturnsAsync(payrollEarnings);
      // Act
      List<PayrollEarningDto> result = await _payrollEarningService.GetAllPayrollEarningsAsync();
      // Assert
      _ = Assert.IsType<List<PayrollEarningDto>>(result);
      Assert.Equal(payrollEarnings.Count, result.Count);
      Assert.Equal(payrollEarnings[0].PayrollEarningId, result[0].PayrollEarningId);
      Assert.Equal(payrollEarnings[1].PayrollEarningId, result[1].PayrollEarningId);
    }

    [Fact]
    public async Task GetPayrollEarningByIdAsyncReturnsPayrollEarning()
    {
      // Arrange
      string payrollEarningId = "PRE001";
      PayrollEarning payrollEarning = new()
      {
        PayrollEarningId = payrollEarningId,
        ShortDescription = "Test Earning",
        LongDescription = "This is a test earning for unit testing.",
        Taxable = true,
        TaxCode = 3601,
        TaxPercentage = 100m,
        OvertimeHourMultiplier = 1.5m,
        CanProRata = false,
        IsOnGoing = false,
      };
      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetByPayrollEarningId(payrollEarningId))
        .ReturnsAsync(payrollEarning);

      // Act
      PayrollEarningDto? result = await _payrollEarningService.GetPayrollEarningByIdAsync(payrollEarningId);

      // Assert
      _ = Assert.IsType<PayrollEarningDto>(result);
      Assert.Equal(payrollEarning.PayrollEarningId, result.PayrollEarningId);
    }

    [Fact]
    public async Task GetPayrollEarningsByTaxCodeReturnsListOfMatchingPayrollEarnings()
    {
      // Arrange
      int taxCode = 3601;
      List<PayrollEarning> payrollEarnings =
      [
        new PayrollEarning
        {
          PayrollEarningId = "PRE001",
          ShortDescription = "Test Earning 1",
          LongDescription = "This is the first test earning for unit testing.",
          Taxable = true,
          TaxCode = taxCode,
          TaxPercentage = 100m,
          OvertimeHourMultiplier = 1.5m,
          CanProRata = false,
          IsOnGoing = false,
        },
        new PayrollEarning
        {
          PayrollEarningId = "PRE002",
          ShortDescription = "Test Earning 2",
          LongDescription = "This is the second test earning for unit testing.",
          Taxable = true,
          TaxCode = taxCode,
          TaxPercentage = 0m,
          OvertimeHourMultiplier = 2m,
          CanProRata = false,
          IsOnGoing = false,
        }
      ];
      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetByTaxCode(taxCode))
        .ReturnsAsync(payrollEarnings);
      // Act
      List<PayrollEarningDto> result = await _payrollEarningService.GetPayrollEarningByTaxCode(taxCode);
      // Assert
      _ = Assert.IsType<List<PayrollEarningDto>>(result);
      Assert.Equal(payrollEarnings.Count, result.Count);
      Assert.All(result, pe => Assert.Equal(taxCode, pe.TaxCode));
    }

    [Fact]
    public async Task UpdatePayrollEarningReturnsUpdatePayrollEarningDetails()
    {
      // Arrange
      string payrollEarningId = "PRE001";
      PayrollEarningUpdateDto payrollEarningUpdateDto = new()
      {
        PayrollEarningId = payrollEarningId,
        ShortDescription = "Updated Test Earning",
        LongDescription = "This is an updated test earning for unit testing.",
        Taxable = false,
        TaxCode = 3602,
        TaxPercentage = 0m,
        OvertimeHourMultiplier = 1m,
        CanProRata = true,
        IsOnGoing = true,
      };
      PayrollEarning updatedPayrollEarning = new()
      {
        PayrollEarningId = payrollEarningId,
        ShortDescription = payrollEarningUpdateDto.ShortDescription,
        LongDescription = payrollEarningUpdateDto.LongDescription,
        Taxable = (bool)payrollEarningUpdateDto.Taxable,
        TaxCode = (int)payrollEarningUpdateDto.TaxCode,
        TaxPercentage = payrollEarningUpdateDto.TaxPercentage,
        OvertimeHourMultiplier = payrollEarningUpdateDto.OvertimeHourMultiplier,
        CanProRata = (bool)payrollEarningUpdateDto.CanProRata,
        IsOnGoing = (bool)payrollEarningUpdateDto.IsOnGoing,
      };

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.CheckIfDescriptionsExists(payrollEarningUpdateDto.ShortDescription, payrollEarningUpdateDto.LongDescription))
        .ReturnsAsync(false);

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.GetByPayrollEarningId(payrollEarningId))
        .ReturnsAsync(updatedPayrollEarning);

      _ = _payrollEarningRepositoryMock
        .Setup(r => r.UpdateAsync(It.IsAny<PayrollEarning>()))
        .ReturnsAsync(updatedPayrollEarning);

      // Act
      PayrollEarningDto result = await _payrollEarningService.UpdatePayrollEarningAsync(payrollEarningUpdateDto);
      // Assert
      _ = Assert.IsType<PayrollEarningDto>(result);
      Assert.Equal(updatedPayrollEarning.PayrollEarningId, result.PayrollEarningId);
      Assert.Equal(updatedPayrollEarning.ShortDescription, result.ShortDescription);
      Assert.Equal(updatedPayrollEarning.LongDescription, result.LongDescription);
      Assert.Equal(updatedPayrollEarning.Taxable, result.Taxable);
      Assert.Equal(updatedPayrollEarning.TaxCode, result.TaxCode);
      Assert.Equal(updatedPayrollEarning.TaxPercentage, result.TaxPercentage);
      Assert.Equal(updatedPayrollEarning.OvertimeHourMultiplier, result.OvertimeHourMultiplier);
      Assert.Equal(updatedPayrollEarning.CanProRata, result.CanProRata);
      Assert.Equal(updatedPayrollEarning.IsOnGoing, result.IsOnGoing);
    }
  }
}
