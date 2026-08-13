namespace HRConnect.Api.Interfaces.Payroll
{
    using HRConnect.Api.DTOs;

    public interface IPayslipService
    {
        Task<IEnumerable<PayslipHistoryDto>> GetPayslipHistoryAsync(
            string employeeId,
            CancellationToken cancellationToken);

        Task<PayslipDto?> GetPayslipAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken);

        Task<PayslipViewDto?> GetPayslipViewAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken);
    }
}