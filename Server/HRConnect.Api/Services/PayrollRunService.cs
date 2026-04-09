namespace HRConnect.Api.Services
{
  using DTOs.Payroll;
  using Interfaces;
  using Mappers.Payroll;
  using Models.Payroll;

  public class PayrollRunService : IPayrollRunService
  {
    private readonly IPayrollRunRepository _payrollRunRepo;
    private readonly IPayrollPeriodService _payrollPeriodService;

    public PayrollRunService(IPayrollRunRepository payrollRunRepo, IPayrollPeriodService payrollPeriodService)
    {
      _payrollRunRepo = payrollRunRepo;
      _payrollPeriodService = payrollPeriodService;
    }

    public async Task<PayrollRunDto?> GetPayrunByRunNumberAsync(int payrollRunNumber)
    {
      var payrun = await _payrollRunRepo.GetPayrunByRunNumberAsync(payrollRunNumber);
      if (payrun == null)
        return null;
      return payrun.ToPayrollRunDto();
    }
    public async Task<IEnumerable<PayrollRunDto>> GetAllPayruns()
    {
      var payruns = await _payrollRunRepo.GetAllPayruns();
      return payruns.Select(p => p.ToPayrollRunDto()).ToList();
    }

    /// <summary>
    ///  Service method used to query and/or get payroll run 'Payroll Run Number', 
    /// 'StartDate' and 'EndDate' packaged in a <see cref="PayrollRunRequestDto"/>
    /// </summary>
    /// <param name="dto">Request DTO with  necessary parameters to request a payroll run</param>
    /// <returns>PayrollRun as a DTO</returns>
    public async Task<PayrollRunDto?> RequestRunByDateAsync(PayrollRunRequestDto dto)
    {
      var requestedRun = await _payrollRunRepo.GetRunByDateAsync(dto.PayrollRunNumber, dto.StartDate, dto.EndDate);
      if (requestedRun == null)
      {
        return null;
      }
      return requestedRun.ToPayrollRunDto();
    }

    public async Task<PayrollRun> GetCurrentRunAsync()
    {
      var payrun = await _payrollRunRepo.GetCurrentRunAsync();

      return payrun!;
    }
    /// <summary>
    /// This service method adds a payroll record type to the current active payroll run's 'Records' collections.
    ///  All records added the collection are automatically locked. 
    /// </summary>
    /// <param name="payrollRecord">Payroll Record derived type being added 
    /// to the current payroll run  </param>
    /// <param name="employeeId">EmployeeId as a foreign for adding record for particular record</param>
    /// <returns>Successfully Completed Task</returns>
    /// <exception cref="InvalidDataException">Invalid Type Expected 'PayrollRecord'
    /// </exception>
    public async Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord, string employeeId)
    {

      var payperiod = await _payrollPeriodService.GetLastPeriodAsync();
      if (payperiod == null)
        throw new InvalidDataException("No payroll period found or it is locked");

      var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();

      if (currentPayRun == null)
        throw new InvalidDataException("No current payroll run found or it is locked");

      payrollRecord.PayrollRun = currentPayRun;
      payrollRecord.EmployeeId = employeeId;
      currentPayRun.Records.Add(payrollRecord);

      await _payrollRunRepo.UpdateRun(currentPayRun);
    }

    public async Task AddRecordsCollectionToRunAsync(IList<PayrollRecord> recordsCollection, string? employeeId)
    {
      var payperiod = await _payrollPeriodService.GetLastPeriodAsync();
      if (payperiod == null)
        throw new InvalidDataException("No payroll period found or it is locked");

      var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();

      if (currentPayRun == null)
        throw new InvalidDataException("No current payroll run found or it is locked");

      foreach (var record in recordsCollection
      )
      {
        record.PayrollRun = currentPayRun;
        record.EmployeeId = employeeId ?? record.EmployeeId;
        currentPayRun.Records.Add(record);
      }
      await _payrollRunRepo.UpdateRun(currentPayRun);
    }
    
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      await _payrollRunRepo.UpdateRun(payrollRun);
    }


    public async Task LockAllOlderPayrollRuns()
    {
      PayrollRun? expiredRun;
      while ((expiredRun = await _payrollRunRepo.IsExpiredPayRunUnlocked()) != null)
      {
        expiredRun.IsLocked = true;
        expiredRun.FinalisedDate = DateTime.Now;
        expiredRun.IsFinalised = false;
        await _payrollRunRepo.UpdateExpiredRun(expiredRun);
      }
    }

    public async Task<int> AddBulkRecordsToCurrentRunAsync(List<PayrollRecord> payrollRecords, List<string> employeeIds)
    {
        // Validate inputs
        if (payrollRecords == null || payrollRecords.Count == 0)
            throw new ArgumentException("Payroll records list cannot be null or empty");
    
        if (employeeIds == null || employeeIds.Count == 0)
            throw new ArgumentException("Employee IDs list cannot be null or empty");
    
        if (payrollRecords.Count != employeeIds.Count)
            throw new ArgumentException("Payroll records and employee IDs lists must have the same length");
    
        // Get the current payroll run ONCE
        var payperiod = await _payrollPeriodService.GetLastPeriodAsync();
        if (payperiod == null)
            throw new InvalidDataException("No payroll period found or it is locked");
    
        var currentPayRun = payperiod.Runs
            .Where(r => !r.IsLocked)
            .OrderByDescending(r => r.PayrollRunNumber)
            .FirstOrDefault();
    
        if (currentPayRun == null)
            throw new InvalidDataException("No current payroll run found or it is locked");
    
        Console.WriteLine($"Processing {payrollRecords.Count} records for payroll run {currentPayRun.PayrollRunNumber}");
    
        // Add all records to the current run
        for (int i = 0; i < payrollRecords.Count; i++)
        {
            var record = payrollRecords[i];
            var employeeId = employeeIds[i];
    
            // Set payroll run and employee ID for each record
            record.PayrollRun = currentPayRun;
            record.EmployeeId = employeeId;
    
            // Add to current run's records collection
            currentPayRun.Records.Add(record);
        }
    
        Console.WriteLine($"Added {payrollRecords.Count} records to payroll run {currentPayRun.PayrollRunNumber}");
    
        // SAVE ONCE - all records at the same time
        await _payrollRunRepo.UpdateRun(currentPayRun);
    
        return payrollRecords.Count;
    }
  }
}