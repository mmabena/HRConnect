namespace HRConnect.Api.Utils.Jobs.Pension
{
  using System.Threading.Tasks;
  using Quartz;
  using HRConnect.Api.Interfaces.Pension;

  public class EmployeeEnrollmentJob(IServiceProvider serviceProvider) : IJob
  {
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    ///<summary>
    ///Intialized quartz job to enroll employee to pension based on their pension option
    ///</summary>
    public async Task Execute(IJobExecutionContext context)
    {
      using IServiceScope scope = _serviceProvider.CreateScope();
      IEmployeePensionEnrollmentService intializer = scope.ServiceProvider.GetRequiredService<IEmployeePensionEnrollmentService>();

      await intializer.InitializeEmployeePensionEnrollment();
    }
  }
}
