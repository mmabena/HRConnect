namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public interface ICompanyContributionAllocationService
    {
        Task AllocateAsync(int payrollRunId);
    }

    public class CompanyContributionAllocationService : ICompanyContributionAllocationService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmployeeCompanyContributionRepository _repo;

        public CompanyContributionAllocationService(
            ApplicationDBContext context,
            IEmployeeCompanyContributionRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public async Task AllocateAsync(int payrollRunId)
        {
            var contributions = await _context.CompanyContributions
                .Where(c => c.IsActive)
                .ToListAsync();

            var death = contributions.FirstOrDefault(c => c.Code == "DEATHBEN");
            var disability = contributions.FirstOrDefault(c => c.Code == "DISABILITY");

            var employees = await _context.Employees
                .Where(e => e.EmploymentStatus == EmploymentStatus.Permanent)
                .ToListAsync();

            var records = new List<EmployeeCompanyContribution>();

            foreach (var emp in employees)
            {
                var salary = emp.MonthlySalary;

var age = DateTime.Today.Year - emp.DateOfBirth.Year;

                var record = new EmployeeCompanyContribution
                {
                    PayrollRunId = payrollRunId,
    EmployeeId = emp.EmployeeId,

    Name = emp.Name,
    Surname = emp.Surname,
    IdNumber = emp.IdNumber,
    PassportNumber = emp.PassportNumber,
    Age = age,
    Salary = salary,

                    DeathPercentage = death?.Percentage ?? 0,
                    DeathAmount = salary * (death?.Percentage ?? 0),

                    DisabilityPercentage = disability?.Percentage ?? 0,
                    DisabilityAmount = salary * (disability?.Percentage ?? 0)
                };

                records.Add(record);
            }

            await _repo.AddRangeAsync(records);
        }
    }
}