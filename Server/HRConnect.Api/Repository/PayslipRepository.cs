namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces.Payroll;
    using HRConnect.Api.Models;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.Models.Payroll;
    using HRConnect.Api.Models.PayrollDeduction;
    using HRConnect.Api.Models.Pension;
    using Microsoft.EntityFrameworkCore;

    public class PayslipRepository(
        ApplicationDBContext context) : IPayslipRepository
    {
        public async Task<PayrollRun?> GetPayrollRunAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default)
        {
            return await context.PayrollRuns
                .Include(x => x.Records)
                .FirstOrDefaultAsync(
                    x =>
                        x.PayrollRunId == payrollRunId &&
                        x.PayrollRunNumber == payrollRunNumber &&
                        x.Records.Any(r => r.EmployeeId == employeeId),
                    cancellationToken);
        }

        public async Task<Employee?> GetEmployeeAsync(
            string employeeId,
            CancellationToken cancellationToken = default)
        {
            return await context.Employees
                .Include(x => x.Position)
                .FirstOrDefaultAsync(
                    x => x.EmployeeId == employeeId,
                    cancellationToken);
        }

        public async Task<PensionFund?> GetPensionFundAsync(
            string employeeId,
            CancellationToken cancellationToken = default)
        {
            return await context.PensionFunds
                .FirstOrDefaultAsync(
                    x => x.EmployeeId == employeeId,
                    cancellationToken);
        }

        public async Task<IEnumerable<EmployeeDeduction>> GetEmployeeDeductionsAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default)
        {
            var payrollRunExists = await context.PayrollRuns
                .AnyAsync(
                    x =>
                        x.PayrollRunId == payrollRunId &&
                        x.PayrollRunNumber == payrollRunNumber,
                    cancellationToken);

            if (!payrollRunExists)
            {
                return Enumerable.Empty<EmployeeDeduction>();
            }

            return await context.EmployeeDeductions
                .Include(x => x.Deduction)
                .Where(
                    x =>
                        x.EmployeeId == employeeId &&
                        x.PayrollRunId == payrollRunId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EmployeeCompanyContribution>> GetCompanyContributionsAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default)
        {
            var payrollRunExists = await context.PayrollRuns
                .AnyAsync(
                    x =>
                        x.PayrollRunId == payrollRunId &&
                        x.PayrollRunNumber == payrollRunNumber,
                    cancellationToken);

            if (!payrollRunExists)
            {
                return Enumerable.Empty<EmployeeCompanyContribution>();
            }

            return await context.EmployeeCompanyContributions
                .Where(
                    x =>
                        x.EmployeeId == employeeId &&
                        x.PayrollRunId == payrollRunId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MedicalAidDeduction>> GetMedicalAidDeductionsAsync(
    string employeeId,
    int payrollRunId,
    int payrollRunNumber,
    CancellationToken cancellationToken = default)
{
    var payrollRunExists = await context.PayrollRuns
        .AnyAsync(
            x =>
                x.PayrollRunId == payrollRunId &&
                x.PayrollRunNumber == payrollRunNumber,
            cancellationToken);

    if (!payrollRunExists)
    {
        return Enumerable.Empty<MedicalAidDeduction>();
    }

    return await context.MedicalAidDeductions
        .Where(x =>
            x.EmployeeId == employeeId &&
            x.PayrollRunId == payrollRunId)
        .ToListAsync(cancellationToken);
}

        public async Task<IEnumerable<PayrollRun>> GetPayslipHistoryAsync(
            string employeeId,
            CancellationToken cancellationToken = default)
        {
            return await context.PayrollRuns
                .Include(x => x.Records)
                .Where(
                    x => x.Records.Any(
                        r => r.EmployeeId == employeeId))
                .OrderByDescending(x => x.PeriodDate)
                .ToListAsync(cancellationToken);
        }
    }
}