namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.Services;

  public class EmployeeEnrollmentJob(IServiceProvider serviceProvider) : IJob
  {
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    ///<summary>
    ///Intialized quartz job to enroll employee to pension based on their pension option
    ///</summary>
    public async Task Execute(IJobExecutionContext context)
    {
      using IServiceScope scope = _serviceProvider.CreateScope();
      EmployeePensionEnrollmentService intializer = scope.ServiceProvider.GetRequiredService<EmployeePensionEnrollmentService>();

      await intializer.InitializeEmployeePensionEnrollment();
    }
  }
}
