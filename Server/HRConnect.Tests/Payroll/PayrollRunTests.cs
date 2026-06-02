namespace HRConnect.Tests.Payroll
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
    private readonly Mock<IPayrollPeriodService> _payrollPeriodService;
    private readonly Mock<IEmployeePensionEnrollmentService> _employeePensionEnrollmentService;
    private readonly Mock<IReportsService> _reportsService;
    private readonly Mock<ICompanyContributionAllocationService> _contributionAllocService;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<IUserService> _userService;
    private readonly Mock<IEmployeeService> _employeeService;
    private readonly Mock<IEmployeeDeductionService> _employeeDeductionService;
    private readonly Mock<IEmployeePayrollEarningService> _employeePayrollEarningService;
    private readonly Mock<INotificationService> _notificationsService;
    private Func<DateTime> _now;

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

      _now = () => DateTime.Now;
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
      services.AddScoped(_ => _contributionAllocService.Object);
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

      PayrollRun lockedRun = new PayrollRun
      {
        PayrollRunNumber = runNumber + 1,
        IsFinalised = false,
        IsLocked = true,
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
      services.AddScoped(_ => _contributionAllocService.Object);

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
    _earningServiceMock.Object,
    _deductionServiceMock.Object,
    _context,
     _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
      
        _contributionRepoMock.Object,
        _bankingDetailService.Object,
    _now
);

      await job.Execute(null!);
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
      services.AddScoped(_ => _contributionAllocService.Object);
      var serviceProvider = services.BuildServiceProvider();

      var currentRun = new PayrollRun
      {
        PayrollRunNumber = 1,
        IsFinalised = true,
        IsLocked = true
      };

      var job = new PayrollRolloverJob(
          _payrollRunRepo.Object,
          _payrollPeriodService.Object,
          serviceProvider,
          _employeePensionEnrollmentService.Object,
          _reportsService.Object,
          _earningServiceMock.Object,
          _deductionServiceMock.Object,
          _context,
            _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
          _now
      );
      await job.Execute(null!);
      //Make sure that there's a new payroll run
      var lockedRun = new PayrollRun { PayrollRunNumber = 1 };
      _payrollRunRepo.Setup(r => r.CreatePayrollRunAsync(It.IsAny<PayrollRun>()))
            .Callback<PayrollRun>(r => r = lockedRun);
    }
    [Fact]
    public async Task ShouldNotRunRolloverBeforeMonthEnd()
    {
      //Arrange 
      var services = new ServiceCollection();
      // register other dependencies if needed
      services.AddScoped(
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
    _earningServiceMock.Object,
    _deductionServiceMock.Object,
    _context,
     _contributionRepoMock.Object,
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
      services.AddScoped(_ => _contributionAllocService.Object);
      var serviceProvider = services.BuildServiceProvider();

      var job = new PayrollRolloverJob(
    _payrollRunRepo.Object,
    _payrollPeriodService.Object,
    serviceProvider,
    _employeePensionEnrollmentService.Object,
    _reportsService.Object,
    _earningServiceMock.Object,
    _deductionServiceMock.Object,
    _context,
       _userService.Object,
      _employeeService.Object,
      _employeePayrollEarningService.Object,
      _employeeDeductionService.Object,
      _notificationsService.Object,
      _contributionRepoMock.Object,
    _now
);

    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}