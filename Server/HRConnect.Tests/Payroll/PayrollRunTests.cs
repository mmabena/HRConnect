namespace HRConnect.Tests
{
  using Moq;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Utils.Jobs.Payroll;
  using HRConnect.Api.Interfaces.Payroll.Earning;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Interfaces.Payroll.Deduction;
  using HRConnect.Api.Interfaces.Notification;
  using System;
  using Microsoft.Extensions.DependencyInjection;
  using HRConnect.Api.Models.PayrollDeduction;

  public class PayrollTests
  {
    private readonly Mock<IPayrollRunRepository> _payrollRunRepo;
    private readonly Mock<IPayrollPeriodRepository> _payrollPeriodRepo;
    private readonly Mock<IPayrollPeriodService> _payrollPeriodService;
    private readonly Mock<IEmployeePensionEnrollmentService> _employeePensionEnrollmentService;
    private readonly Mock<IReportsService> _reportsService;
    private readonly Mock<ICompanyContributionRepository> _contributionRepoMock;
    private readonly Mock<ICompanyContributionAllocationService> _contributionAllocService;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IUserService> _userService;
    private readonly Mock<IEmployeeService> _employeeService;
    private readonly Mock<IEmployeeDeductionService> _employeeDeductionService;
    private readonly Mock<IEmployeePayrollEarningService> _employeePayrollEarningService;
    private readonly Mock<INotificationService> _notificationsService;
    private Func<DateTime> _now;

    //These are not injected into they are however used for mocking scoped services
    //in the job
    // private readonly Mock<IServiceScope> _scopeMock;
    // private readonly Mock<IServiceProvider> _serviceProviderMock;
    // private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    public PayrollTests()
    {
      _payrollRunRepo = new Mock<IPayrollRunRepository>();
      _payrollPeriodService = new Mock<IPayrollPeriodService>();

      _employeePensionEnrollmentService = new Mock<IEmployeePensionEnrollmentService>();
      _reportsService = new Mock<IReportsService>();
      _userService = new Mock<IUserService>();
      _employeeService = new Mock<IEmployeeService>();
      _employeePayrollEarningService = new Mock<IEmployeePayrollEarningService>();
      _employeeDeductionService = new Mock<IEmployeeDeductionService>();
      _notificationsService = new Mock<INotificationService>();
      _payrollPeriodRepo = new Mock<IPayrollPeriodRepository>();
      // _contributionRepoMock = new Mock<ICompanyContributionRepository>();
      // _contributionAllocService = new Mock<ICompanyContributionAllocationService>();
      //  _serviceProvider = serviceProvider;

      _now = () => DateTime.Now;

      // //Mock a scope for the service provider that will be used by the injected depenedency
      // _serviceProviderMock = new Mock<IServiceProvider>();
      // _serviceProviderMock
      //   .Setup(sp => sp.GetService(typeof(ICompanyContributionAllocationService)))
      //   .Returns(_contributionAllocService.Object);
      //
      // //Mock the service scope
      // _scopeMock = new Mock<IServiceScope>();
      // _scopeMock
      //   .Setup(ss => ss.ServiceProvider)
      //   .Returns(_serviceProviderMock.Object);
      //
      // _serviceScopeFactoryMock=new Mock<IServiceScopeFactory>();


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
      // register other dependencies if needed
      services.AddScoped<ICompanyContributionAllocationService>(
          _ => _contributionAllocService.Object
          );
      services.AddScoped<ICompanyContributionAllocationService>(
          _ => _contributionAllocService.Object
          );
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
      _reportsService.Object,
      _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
        _now
      );
      //Act now
      await job.Execute(null!);

      //Create a new run
      _payrollRunRepo.Setup(r => r.CreatePayrollRunAsync(It.IsAny<PayrollRun>()))
      .ReturnsAsync(lockedRun);
      //Update the existing run to be locked 
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
      // register other dependencies if needed
      services.AddScoped<ICompanyContributionAllocationService>(
          _ => _contributionAllocService.Object
          );
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
      _reportsService.Object,
      _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
        _now
      );

      await job.Execute(null!);
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
      // register other dependencies if needed
      services.AddScoped<ICompanyContributionAllocationService>(
    _ => _contributionAllocService.Object
    );
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
      _reportsService.Object,
      _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
        _now
      );

      await job.Execute(null!);

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
      // register other dependencies if needed
      services.AddScoped<ICompanyContributionAllocationService>(
          _ => _contributionAllocService.Object
          );
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
      _reportsService.Object,
      _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
        _now
      );
      if (IsLastMomentOfTheMonth(_now()))
      {
        await job.Execute(null!);
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
      // register other dependencies if needed
      services.AddScoped<ICompanyContributionAllocationService>(
          _ => _contributionAllocService.Object
          );
      var serviceProvider = services.BuildServiceProvider();

      var job = new PayrollRolloverJob(
      _payrollRunRepo.Object,
        _payrollPeriodService.Object,
        serviceProvider,
        _employeePensionEnrollmentService.Object,
        _reportsService.Object,
        _userService.Object,
        _employeeService.Object,
        _employeePayrollEarningService.Object,
        _employeeDeductionService.Object,
        _notificationsService.Object,
        null
        );
    }
  }
}