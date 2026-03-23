using Xunit;
using Moq;
using System;
using HRConnect.Api.Models.Payroll;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Utils.Payroll;
using Quartz;
public class PayrollTests
{
  private readonly Mock<IPayrollRunRepository> _payrollRunRepo;
  private readonly Mock<IPayrollPeriodRepository> _payrollPeriodRepo;
  private readonly Mock<IPayrollPeriodService> _payrollPeriodService;
  private readonly Mock<IWebHostEnvironment> _env;
  private readonly Func<DateTime> _now;
  //create a report Service

  public PayrollTests()
  {
    _payrollRunRepo = new Mock<IPayrollRunRepository>();
    _payrollPeriodService = new Mock<IPayrollPeriodService>();
    _payrollPeriodRepo = new Mock<IPayrollPeriodRepository>();
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
  public void Should_Finalise_And_Create_New_Payroll_Run_Execute()
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
        _env.Object);
    //Act now
    await job.Execute(null);
    //Assert we got the results we wanted
    //
    //
    //
    Assert.All(newRun.Records, r => Assert.True(r.IsLocked));
    _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<PayrollRun>()), Times.Never);

  }
}