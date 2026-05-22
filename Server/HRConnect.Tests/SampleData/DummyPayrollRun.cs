namespace HRConnect.Tests.SampleData;

using HRConnect.Api.Models.Payroll;

/// <summary>
/// Sample payroll run record for testing purposes
/// </summary>
public static class DummyPayrollRun
{
    /// <summary>
    /// Creates a realistic dummy payroll run record matching the actual PayrollRun entity
    /// </summary>
    /// <param name="payrollRunId">Optional custom payroll run ID</param>
    /// <param name="payrollRunNumber">Optional custom payroll run number</param>
    /// <param name="periodId">Optional custom period ID</param>
    /// <param name="isFinalised">Whether the run should be finalized</param>
    /// <param name="isLocked">Whether the run should be locked</param>
    /// <returns>A dummy PayrollRun object</returns>
    public static PayrollRun Create(int? payrollRunId = null, int? payrollRunNumber = null, int? periodId = null, bool isFinalised = false, bool isLocked = false)
    {
        var runId = payrollRunId ?? new Random().Next(1000, 9999);
        var runNumber = payrollRunNumber ?? runId;
        var pId = periodId ?? 1;
        
        return new PayrollRun
        {
            PayrollRunId = runId,
            PayrollRunNumber = runNumber,
            PeriodId = pId,
            PeriodDate = new DateTime(2024, 4, 1), // April 1, 2024
            IsFinalised = isFinalised,
            IsLocked = isLocked,
            FinalisedDate = isFinalised ? DateTime.Now : (DateTime?)null,
            Period = new PayrollPeriod
            {
                PayrollPeriodId = pId,
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2024, 4, 30),
                IsClosed = false,
                IsLocked = false
            },
            Records = new List<PayrollRecord>() // Can be populated with dummy records if needed
        };
    }

    /// <summary>
    /// Creates a finalized payroll run from the previous month
    /// </summary>
    /// <returns>A finalized PayrollRun object</returns>
    public static PayrollRun CreatePreviousMonthRun()
    {
        return new PayrollRun
        {
            PayrollRunId = 1234,
            PayrollRunNumber = 1234,
            PeriodId = 12, // Assuming March is period 12
            PeriodDate = new DateTime(2024, 3, 1), // March 1, 2024
            IsFinalised = true,
            IsLocked = true,
            FinalisedDate = new DateTime(2024, 4, 5), // Finalized on April 5
            Period = new PayrollPeriod
            {
                PayrollPeriodId = 12,
                StartDate = new DateTime(2024, 3, 1),
                EndDate = new DateTime(2024, 3, 31),
                IsClosed = true,
                IsLocked = true
            },
            Records = new List<PayrollRecord>()
        };
    }

    /// <summary>
    /// Creates a minimal payroll run for basic testing
    /// </summary>
    /// <returns>A minimal PayrollRun object</returns>
    public static PayrollRun CreateMinimal()
    {
        return new PayrollRun
        {
            PayrollRunId = 1,
            PayrollRunNumber = 1,
            PeriodId = 1,
            PeriodDate = DateTime.Now,
            IsFinalised = false,
            IsLocked = false,
            FinalisedDate = null,
            Period = null!, // Can be null for minimal testing
            Records = new List<PayrollRecord>()
        };
    }

    /// <summary>
    /// Creates an active current payroll run (not finalized, not locked)
    /// </summary>
    /// <returns>An active PayrollRun object</returns>
    public static PayrollRun CreateActiveRun()
    {
        return new PayrollRun
        {
            PayrollRunId = 9999,
            PayrollRunNumber = 9999,
            PeriodId = 1,
            PeriodDate = new DateTime(2024, 4, 1),
            IsFinalised = false,
            IsLocked = false,
            FinalisedDate = null,
            Period = new PayrollPeriod
            {
                PayrollPeriodId = 1,
                StartDate = new DateTime(2024, 4, 1),
                EndDate = new DateTime(2024, 4, 30),
                IsClosed = false,
                IsLocked = false
            },
            Records = new List<PayrollRecord>()
        };
    }
}
