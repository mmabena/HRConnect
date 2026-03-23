namespace HRConnect.Api.Utils.Quartz.Pension
{
  using System.Threading.Tasks;
  using global::Quartz;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using Microsoft.EntityFrameworkCore;

  [DisallowConcurrentExecution]
  public class PensionDeductionJob(IPensionDeductionRepository pensionDeductionRepository, 
    IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository, IEmployeeRepository employeeRepository,
    ApplicationDBContext context) : IJob
  {
    private readonly IPensionDeductionRepository _pensionDeductionRepository = pensionDeductionRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly ApplicationDBContext _context = context;

    public async Task Execute(IJobExecutionContext context)
    {
      Console.Write(":-) GOOD LUCK DELETING THIS QUARTZ METHOD ;-)");
      await Task.CompletedTask;
    }


  }
}
