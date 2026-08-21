namespace HRConnect.Api.Services
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.CompanyContributions;
  using HRConnect.Api.Models.Payroll;
  using Microsoft.EntityFrameworkCore;

  public class CompanyContributionAllocationService : ICompanyContributionAllocationService
  {
    private readonly ApplicationDBContext _context;
    private readonly IEmployeeCompanyContributionRepository _CompanyContributionRepo;

    public CompanyContributionAllocationService(
        ApplicationDBContext context,
        IEmployeeCompanyContributionRepository CompanyContributionRepo)
    {
      _context = context;
      _CompanyContributionRepo = CompanyContributionRepo;
    }
    /// <summary>
    /// Allocates company contribution records for eligible employees.
    /// </summary>
    /// <param name="payrollRunId">The payroll run identifier for which to allocate contributions.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<List<PayrollRecord>> AllocateAsync(int payrollRunId)
    {
      var payrun = await _context.PayrollRuns.FirstOrDefaultAsync(p => p.PayrollRunId == payrollRunId);

      if (payrun == null || payrun.IsLocked)
        throw new InvalidOperationException("Payroll run is locked or not found");

      var existingEmployeeIds = await _context.EmployeeCompanyContributions
          .Where(e => e.PayrollRunId == payrollRunId)
          .Select(e => e.EmployeeId)
          .ToHashSetAsync();


      var contributions = await _context.CompanyContributions
          .Where(c => c.IsActive)
          .ToListAsync();

      var bee = contributions.FirstOrDefault(c => c.Code == "BEE7");
      var death = contributions.FirstOrDefault(c => c.Code == "DEATHBEN");
      var disability = contributions.FirstOrDefault(c => c.Code == "DISABILITY");

      var today = DateOnly.FromDateTime(DateTime.Today);

      var employees = await _context.Employees
          .Where(e => e.EmploymentStatus == EmploymentStatus.Permanent)
          .ToListAsync();

      var payrollRecords = new List<PayrollRecord>();
      var newRecords = new List<EmployeeCompanyContribution>();

      foreach (var emp in employees)
      {
        if (existingEmployeeIds.Contains(emp.EmployeeId))
          continue;

        var salary = emp.MonthlySalary;

        var age = today.Year - emp.DateOfBirth.Year;
        if (emp.DateOfBirth > today.AddYears(-age)) age--;

        if (age >= 65)
          continue;

        var beePercentage = bee?.Percentage ?? 0;
        var deathPercentage = death?.Percentage ?? 0;
        var disabilityPercentage = disability?.Percentage ?? 0;

        var contribution = new EmployeeCompanyContribution
        {
          PayrollRunId = payrollRunId,
          EmployeeId = emp.EmployeeId,
          IsLocked = false,

          Name = emp.Name,
          Surname = emp.Surname,
          IdNumber = emp.IdNumber!,
          PassportNumber = emp.PassportNumber!,
          Age = age,
          Salary = salary,

          BEEPercentage = beePercentage,
          BEEAmount = salary * (beePercentage / 100m),

          DeathPercentage = deathPercentage,
          DeathAmount = salary * deathPercentage,

          DisabilityPercentage = disabilityPercentage,
          DisabilityAmount = salary * disabilityPercentage
        };

        newRecords.Add(contribution);
        payrollRecords.Add(contribution);
      }

      if (newRecords.Count > 0)
      {
        await _CompanyContributionRepo.AddRangeAsync(newRecords);
      }
      return payrollRecords;
    }
  }
}