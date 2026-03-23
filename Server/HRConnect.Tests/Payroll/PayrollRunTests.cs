using Xunit;
using Moq;
using System;
using HRConnect.Api.Models.Payroll;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Utils.Payroll;
using Quartz;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
public class PayrollTests
{
  private readonly Mock<IPayrollRunRepository> _payrollRunRepo;
  private readonly Mock<IPayrollPeriodRepository> _payrollPeriodRepo;
  private readonly Mock<IPayrollPeriodService> _payrollPeriodService;
  private readonly Mock<IReportsService> _reportsService;
  private readonly Mock<IWebHostEnvironment> _env;
  private readonly Func<DateTime> _now;
  //create a report Service

  public PayrollTests()
  {
    _payrollRunRepo = new Mock<IPayrollRunRepository>();
    _payrollPeriodService = new Mock<IPayrollPeriodService>();
    _payrollPeriodRepo = new Mock<IPayrollPeriodRepository>();
    _reportsService = new Mock<IReportsService>();
    _now = (() => DateTime.Now);
    _env = new Mock<IWebHostEnvironment>();
  }

  //Will be used to mock the time of roll over
  private bool IsLastMomentOfTheMonth(DateTime dateTime)
  {
    var lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

    return dateTime.Day == lastDay &&
      dateTime.Hour == 23 && dateTime.Minute == 59;
  }

  [Fact]
  public async Task Should_Finalise_And_Create_New_Payroll_Run_Execute()
  {
    // var repo = new Mock<IPayrollRunRepository>();
    // var _env = new Mock<IWebHostEnvironment>();
    var currentRun = new PayrollRun
    {
      PayrollRunId = 1,
      PayrollRunNumber = ((DateTime.Now.Month + 8) % 12) + 1,
      PeriodDate = new DateTime(2026, 3, 1),
      IsFinalised = false,
      IsLocked = false,
      Records = new List<PayrollRecord>
      {
        // new PayrollRecord{IsLocked=false}
      }
    };
    var period = new PayrollPeriod
    {
      Runs = new List<PayrollRun> { currentRun }
    };
    _payrollPeriodService.Setup(p => p.GetLastPeriodAsync()).ReturnsAsync(period);
    _payrollRunRepo.Setup(r => r.GetCurrentRunAsync()).ReturnsAsync(currentRun);

    PayrollRun newRun = null!;

    _payrollRunRepo.Setup(r => r.CreatePayrollRunAsync(It.IsAny<PayrollRun>()))
          .Callback<PayrollRun>(r => newRun = r);

    //fake future time
    var fakeTime = new DateTime(2026, 3, 31, 23, 59, 59);//23:59 March 31st
    var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        _env.Object,
        _reportsService.Object);
    //Act now
    await job.Execute(null);
    //Assert we got the results we wanted
    Assert.True(newRun.IsLocked);
    Assert.True(newRun.IsClosed);
    Assert.All(newRun.Records, r => Assert.True(r.IsLocked));
    //Make sure we check if the updates have applied
    _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<PayrollRun>()), Times.Never);
  }

  [Fact]
  public async Task Execute_Should_Create_New_Run_If_None_Exist()
  {
    var period = new PayrollPeriod
    {
      Runs = new List<PayrollRun>()//No runs in this period yet
    };

    _payrollPeriodService.Setup(period => period.GetLastPeriodASync())
      .ReturnsAsync(period); ;

    var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        _env.Object,
        _reportsService.Object,
        fakeTime);

    await job.Execute(null);
    //I'll attend the asserts at a later stage


  }
  [Fact]
  public async Task Execute_Should_Not_Allow_Modify_On_Locked()
  {
    var currentRun = new PayrollRun
    {
      PayrollRunNumber = 1,
      IsFinalised = true,
      IsClose = true
    };

    var period = new PayrollPeriod
    {
      Runs = new List<PayrollRun> { currentRun }
    };

    _payrollPeriodService.Setup(period => period.GetLastPeriodAsync())
    .ReturnAsync(period);

    var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        _env.Object,
        _reportsService.Object,
        fakeTime);

    await job.Execute(null);
    _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<_payrollRunRepo>()), Times.Never);
  }
}