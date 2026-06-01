namespace HRConnect.Api.Utils.Jobs
{
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces.Payroll.Earning;
  using HRConnect.Api.Interfaces.Pension;
  using Quartz;

  public class EmployeeEnrollmentJob(IServiceProvider serviceProvider) : IJob
  {
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    ///<summary>
    ///Intialized quartz job to enroll employee to pension based on their pension option
    ///</summary>
    public async Task Execute(IJobExecutionContext context)
    {
      using IServiceScope scope = _serviceProvider.CreateScope();
      IEmployeePensionEnrollmentService pensionInitializer = scope.ServiceProvider.GetRequiredService<IEmployeePensionEnrollmentService>();
      IEmployeePayrollEarningService payrollEarningInitializer = scope.ServiceProvider.GetRequiredService<IEmployeePayrollEarningService>();

      await pensionInitializer.InitializeEmployeePensionEnrollment();
      await payrollEarningInitializer.InitializeEmployeePayrollEarningsAsync();
    }
  }
}
