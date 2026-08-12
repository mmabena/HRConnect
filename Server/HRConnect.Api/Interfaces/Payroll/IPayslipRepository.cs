namespace HRConnect.Api.Interfaces.Payroll
{
    using HRConnect.Api.Models;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.Models.Payroll;
    using HRConnect.Api.Models.PayrollDeduction;

    public interface IPayslipRepository
    {
        Task<PayrollRun?> GetPayrollRunAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default);

        Task<Employee?> GetEmployeeAsync(
            string employeeId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<EmployeeDeduction>> GetEmployeeDeductionsAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<EmployeeCompanyContribution>> GetCompanyContributionsAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<PayrollRun>> GetPayslipHistoryAsync(
            string employeeId,
            CancellationToken cancellationToken = default);
    }
}