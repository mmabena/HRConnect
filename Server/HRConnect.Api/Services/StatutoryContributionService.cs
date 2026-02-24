
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.DTOs.StatutoryContribution;
  using Audit.Core;

  public class StatutoryContributionService : IStatutoryContributionsService
  {
    private readonly IEmployeeRepository _employeeRepo;
    private readonly StatutoryContributionsCalculator _deductionsCalculator;
    private readonly IStatutoryContributionRepository _statutoryContributionRepo;
    public StatutoryContributionService(IEmployeeRepository employeeRepo, IStatutoryContributionRepository payrollContributionsRepo)
    {
      _employeeRepo = employeeRepo;
      _statutoryContributionRepo = payrollContributionsRepo;
      _deductionsCalculator = new StatutoryContributionsCalculator();
    }

    public async Task<StatutoryContributionDto?> GetDeductionsByEmployeeIdAsync(string employeeId)
    {
      var deduction = await _statutoryContributionRepo.GetDeductionsByEmployeeIdAsync(employeeId);
      if (deduction == null)
        return null;
      return deduction.ToPayrollDeductionDto();
    }
    public async Task<IEnumerable<StatutoryContributionDto>> GetAllDeductionsAsync()
    {
      var deductions = await _statutoryContributionRepo.GetAllDeductionsAsync();
      return deductions.Select(d => d.ToPayrollDeductionDto()).ToList();
    }
    /// <summary>
    /// This method adds and entry for payroll deductions. It further tracks and audits deductions using a scoped approach
    /// </summary>
    /// <param name="employeeId">Id of employee whose salary deductions are calculated</param>
    /// <returns>Newly added deductions</returns>
    /// <exception cref="KeyNotFoundException">Thrown when employeeId does not return an existing employee</exception>
    /// <exception cref="ArgumentException">Generic exception should this method fail with an existing employee</exception>
    public async Task<StatutoryContribution?> AddDeductionsAsync(string employeeId)
    {

      using (var scope = AuditScope.Create("StatutoryContribution:Insert",
            () => new { employeeId }, EventCreationPolicy.InsertOnEnd))
      {
        try
        {
          Employee? employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
          if (employee == null) throw new KeyNotFoundException();

          var (employeeAmount, employerAmount) = _deductionsCalculator.CalculateUif(employee.MonthlySalary);
          var sdlDeduction = _deductionsCalculator.CalculateSdlAmount(employee.MonthlySalary);
          var deductions = new StatutoryContribution
          {
            EmployeeId = employee.EmployeeId,
            MonthlySalary = employee.MonthlySalary,
            PassportNumber = employee.PassportNumber,
            IdNumber = employee.IdNumber,
            UifEmployeeAmount = employeeAmount,
            UifEmployerAmount = employerAmount,
            EmployerSdlContribution = sdlDeduction,
            CurrentMonth = DateTime.UtcNow.AddMonths(1) //adds a month with automatic rollover

          };

          var newDeduction = await _statutoryContributionRepo.AddDeductionsAsync(deductions);
          decimal projectedSalary = newDeduction.MonthlySalary - newDeduction.UifEmployeeAmount;

          scope.Event.Environment = null;

          return newDeduction;
        }

        catch (ArgumentException ex)
        {
          //Discards audit capturing should no deduction take place
          scope.Discard();
          throw new ArgumentException($"Failed To Add deductions {ex.Message}");
        }
      }
    }
  }
}
