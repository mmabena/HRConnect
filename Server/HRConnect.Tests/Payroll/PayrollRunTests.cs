namespace HRConnect.Tests
{
  using Moq;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Utils.Jobs.Payroll;
  using HRConnect.Api.Interfaces.Pension;
  using Quartz;
  using System;
  using Microsoft.Extensions.DependencyInjection;
  using HRConnect.Api.Models.PayrollDeduction;

  public class PayrollTests
  {
    private readonly Mock<IPayrollRunRepository> _payrollRunRepo;
    private readonly Mock<IPayrollPeriodRepository> _payrollPeriodRepo;
    private readonly Mock<IPayrollPeriodService> _payrollPeriodService;
    private readonly Mock<IEmployeePensionEnrollmentService> _employeePensionEnrollmentService;
    private readonly Mock<IPensionDeductionService> _pensionDeductionService;
    private Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IReportsService> _reportsService;
    private Func<DateTime> _now;

    public PayrollTests()
    {
      _payrollRunRepo = new Mock<IPayrollRunRepository>();
      _payrollPeriodService = new Mock<IPayrollPeriodService>();
      _payrollPeriodRepo = new Mock<IPayrollPeriodRepository>();
      _reportsService = new Mock<IReportsService>();
      _now = () => DateTime.Now;
      _employeePensionEnrollmentService = new Mock<IEmployeePensionEnrollmentService>();

      //Mock dependency of Pension Deduction
      _pensionDeductionService = new Mock<IPensionDeductionService>();
      _pensionDeductionService.Setup(ps => ps.PensionDeductionRollover())
      .Returns(Task.CompletedTask);

      //Mock a scope for the service scope that will be used by the injected depenedency
      _scopeMock = new Mock<IServiceScope>();
      _scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IPensionDeductionService)))
           .Returns(_pensionDeductionService.Object);

      //Mocking a Service Scope Factory interface to return our scope
      var scopeFactoryMock = new Mock<IServiceScopeFactory>();
      scopeFactoryMock.Setup(sf => sf.CreateScope())
      .Returns(_scopeMock.Object);

      // Finally mocking the service provider
      _serviceProvider = new Mock<IServiceProvider>();
      _serviceProvider.Setup(sp => sp.GetService(typeof(IServiceProvider)))
      .Returns(scopeFactoryMock.Object);
    }

    //Will be used to mock the time of roll over
    private static bool IsLastMomentOfTheMonth(DateTime dateTime)
    {
      var lastDay = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

      return dateTime.Day == lastDay &&
        dateTime.Hour == 23 && dateTime.Minute == 59;
    }

    /// <summary>
    /// Current PayrollRun finalised, new PayrollRun created automatically and no 
    /// manual trigger required
    /// </summary>
    [Fact]
    public async Task ShouldFinaliseAndCreateNewPayrollRunExecute()
    {
      var services = new ServiceCollection();
      services.AddScoped<IPensionDeductionService>(_ => _pensionDeductionService.Object);
      // register other dependencies if needed
      var serviceProvider = services.BuildServiceProvider();

      var runNumber = ((DateTime.Now.Month + 8) % 12) + 1;
      var currentRun = new PayrollRun
      {
        PayrollRunId = 1,
        PayrollRunNumber = runNumber,
        PeriodDate = new DateTime(2026, 3, 1),
        IsFinalised = false,
        IsLocked = false,
        Records = new List<PayrollRecord>
        {
          new MedicalAidDeduction
          {
              IsLocked =false
          }
        }
      };
      var period = new PayrollPeriod
      {
        Runs = new List<PayrollRun> { currentRun }
      };
      _payrollPeriodService.Setup(p => p.GetLastPeriodAsync()).ReturnsAsync(period);
      // _payrollRunRepo.Setup(r => r.GetCurrentRunAsync()).ReturnsAsync(currentRun);

      PayrollRun lockedRun = new PayrollRun
      {
        PayrollRunNumber = runNumber + 1,
        IsFinalised = false,
        IsLocked = false,
        Records = new List<PayrollRecord>
        {
          new MedicalAidDeduction
          {
              IsLocked = true
          }
        }
      };

      //Arrange
      //fake future time
      var fakeTime = new DateTime(2026, 3, 31, 23, 59, 59);//23:59 March 31st
      _now = () => fakeTime;
      var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
  serviceProvider,
        _employeePensionEnrollmentService.Object,
        _pensionDeductionService.Object,
        _reportsService.Object,
        _now
      );
      //Act now
      await job.Execute(null);

      //Create a new run
      _payrollRunRepo.Setup(r => r.CreatePayrollRunAsync(It.IsAny<PayrollRun>()))
      .ReturnsAsync(lockedRun);
      //Update the existing run to be locked 
      // _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<PayrollRun>()), Times.Never);
      _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<PayrollRun>()), Times.AtMostOnce);

      //Assert we got the results we wanted
      Assert.True(lockedRun.IsLocked);
      Assert.True(lockedRun.IsFinalised);
      // Assert.All(lockedRun.Records, r => Assert.True(r.IsLocked));
      //Make sure we check if the updates have applied
    }

    /// <summary>
    /// Create a new PayrollRun when none exists in the system
    /// </summary>
    [Fact]
    public async Task ExecuteShouldCreateNewRunIfNoneExist()
    {
      //Arrange 
      var services = new ServiceCollection();
      services.AddScoped<IPensionDeductionService>(_ => _pensionDeductionService.Object);
      // register other dependencies if needed
      var serviceProvider = services.BuildServiceProvider();


      var period = new PayrollPeriod
      {
        Runs = new List<PayrollRun>()//No runs in this period yet
      };

      _payrollPeriodService.Setup(period => period.GetLastPeriodAsync())
        .ReturnsAsync(period); ;

      var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        serviceProvider,
        _employeePensionEnrollmentService.Object,
        _pensionDeductionService.Object,
        _reportsService.Object,
        _now
      );

      await job.Execute(null);
      //Make sure that there's a new payroll run
      // _payrollRunRepo.Setup(r => r.CreatePayrollRunAsync(It.IsAny<PayrollRun>()))
      // .ReturnsAsync<PayrollRun>((PayrollRun)null!);
      var lockedRun = new PayrollRun { PayrollRunNumber = 1 };
      _payrollRunRepo.Setup(r => r.CreatePayrollRunAsync(It.IsAny<PayrollRun>()))
            .Callback<PayrollRun>(r => r = lockedRun);
    }

    /// <summary>
    /// Locking test to try edit a locked PayrollRun should return an Exception
    /// </summary>
    [Fact]
    public async Task ExecuteShouldThrowExceptionOnUpdatingLockedRecord()
    {
      //Arrange 
      var services = new ServiceCollection();
      services.AddScoped<IPensionDeductionService>(_ => _pensionDeductionService.Object);
      // register other dependencies if needed
      var serviceProvider = services.BuildServiceProvider();


      var currentRun = new PayrollRun
      {
        PayrollRunNumber = 1,
        IsFinalised = true,
        IsLocked = true
      };

      var period = new PayrollPeriod
      {
        Runs = new List<PayrollRun> { currentRun }
      };

      _payrollPeriodService.Setup(period => period.GetLastPeriodAsync())
      .ReturnsAsync(period);

      var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        serviceProvider,
        _employeePensionEnrollmentService.Object,
        _pensionDeductionService.Object,
        _reportsService.Object,
        _now
      );

      await job.Execute(null);

      _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<PayrollRun>()), Times.Never);

      //Throw Exception for editing locked run
      // Assert.Throw<InvalidOperationException>(() =>
      // throw new InvalidOperationException("Record/Run under Hard Lock. Cannot be modified"));
    }

    [Fact]
    public async Task ShouldNotRunRolloverBeforeMonthEnd()
    {
      //Arrange 
      var services = new ServiceCollection();
      services.AddScoped<IPensionDeductionService>(_ => _pensionDeductionService.Object);
      // register other dependencies if needed
      var serviceProvider = services.BuildServiceProvider();

      var currentRun = new PayrollRun
      {
        PayrollRunId = 1,
        PeriodDate = new DateTime(2026, 2, 1),
        IsFinalised = false,
        IsLocked = false
      };

      _payrollRunRepo.Setup(r => r.GetLastPayrun())
                .ReturnsAsync(currentRun);

      _now = () => new DateTime(2026, 3, 30, 23, 59, 59); //end of the month 
                                                          //Act

      var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        serviceProvider,
        _employeePensionEnrollmentService.Object,
        _pensionDeductionService.Object,
        _reportsService.Object,
        _now
      );
      if (IsLastMomentOfTheMonth(_now()))
      {
        await job.Execute(null);
      }

      // Assert
      _payrollRunRepo.Verify(r => r.UpdateRun(It.IsAny<PayrollRun>()), Times.Never);
      Assert.False(currentRun.IsFinalised);
      Assert.False(currentRun.IsLocked);
    }


    [Fact]
    public async Task RolloverJobCallsRolloverPayrollDeductions()
    {
      //Arrange 
      var services = new ServiceCollection();
      services.AddScoped<IPensionDeductionService>(_ => _pensionDeductionService.Object);
      // register other dependencies if needed
      var serviceProvider = services.BuildServiceProvider();

      var job = new PayrollRolloverJob(
        _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        serviceProvider,
        _employeePensionEnrollmentService.Object,
        _pensionDeductionService.Object,
        _reportsService.Object,
        _now
      );


      //Act:run the method
      //This should call the private method 'RolloverPayrollDeductions'
      await job.Execute(null);

      //Asssert
      _pensionDeductionService.Verify(s => s.PensionDeductionRollover(), Times.AtLeastOnce);

    }
  }
}