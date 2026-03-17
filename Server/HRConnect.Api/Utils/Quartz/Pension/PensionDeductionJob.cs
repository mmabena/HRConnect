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
      List<EmployeePensionEnrollment> employeePensionEnrollments = await _employeePensionEnrollmentRepository.GetEmployeePensionEnrollmentsNotLocked();

      foreach (EmployeePensionEnrollment enrollment in employeePensionEnrollments)
      {
        Employee? employee = await _employeeRepository.GetEmployeeByIdAsync(enrollment.EmployeeId);
        if (employee != null)
        {
          decimal pensionCategoryPercentage = await _context.PensionOptions
          .Where(po => po.PensionOptionId == employee.PensionOptionId)
          .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();

          PensionDeduction pensionDeduction = new()
          {
            EmployeeId = enrollment.EmployeeId,
            FirstName = employee.Name,
            LastName = employee.Surname,
            DateJoinedCompany = employee.StartDate,
            IDNumber = employee.IdNumber,
            Passport = employee.PassportNumber,
            TaxNumber = employee.TaxNumber,
            PensionableSalary = employee.MonthlySalary,
            PensionOptionId = enrollment.PensionOptionId,
            PendsionCategoryPercentage = pensionCategoryPercentage,
            PensionContribution = Math.Round(employee.MonthlySalary * (pensionCategoryPercentage / 100)),
            VoluntaryContribution = enrollment.VoluntaryContribution,
            EmailAddress = employee.Email,
            PhyscialAddress = employee.PhysicalAddress,
            CreatedDate = DateOnly.FromDateTime(DateTime.Now),
            PayrollRunId = enrollment.PayrollRunId,
            IsActive = true
          };

          _ = await _pensionDeductionRepository.AddAsync(pensionDeduction);
        }
      }
    }


  }
}
