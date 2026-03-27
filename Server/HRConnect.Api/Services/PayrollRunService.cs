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

    public async Task<PayrollRunDto?> GetPayrunByRunNumberAsync(int id)
    {
      var payrun = await _payrollRunRepo.GetPayrunByRunNumberAsync(id);
      if (payrun == null)
        return null;
      return payrun.ToPayrollRunDto();
    }
    public async Task<IEnumerable<PayrollRunDto>> GetAllPayruns()
    {
      var payruns = await _payrollRunRepo.GetAllPayruns();
      return payruns.Select(p => p.ToPayrollRunDto()).ToList();
    }
    /// CONSIDER CHANGING THE RETURN TYPE OF THIS TASK
    public async Task<PayrollRun> CreatePayrollRunAsync(PayrollRun payrollRun)
    {
      DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
      Console.WriteLine($"Creating payroll run for month: {currentMonth}");
      //maps current financial month to 1-12
      payrollRun.IsLocked = false;
      payrollRun.IsFinalised = false;
      payrollRun.PeriodDate = currentMonth;

      await _payrollRunRepo.CreatePayrollRunAsync(payrollRun);
      return payrollRun;
    }
    public Task<PayrollRunDto?> GetRunByDateAsync(DateTime dateTime)
    {
      throw new NotImplementedException();
    }
    public async Task<PayrollRun> GetCurrentRunAsync()
    {
      var payrun = await _payrollRunRepo.GetCurrentRunAsync();

      return payrun!;
    }

    public async Task AddRecordToCurrentRunAsync(PayrollRecord payrollRecord, string employeeId)
    {
      // var currentRun = await _payrollRunRepo.GetCurrentRunAsync();
      // if (currentRun == null)
      //   return;
      // currentRun.Records!.Add(payrollRecord); //records may be null
      var payperiod = await _payrollPeriodService.GetLastPeriodAsync();
      if (payperiod == null)
        throw new InvalidDataException("No payroll period found or it is locked");

      var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();

      if (currentPayRun == null)
        throw new InvalidDataException("No current payroll run found or it is locked");

      Console.WriteLine($"!!!!!-+++++++++--------Adding record to current run with ID {currentPayRun.PayrollRunNumber}");
      payrollRecord.PayrollRun = currentPayRun; //shoule have current payrollRunNumber
      payrollRecord.EmployeeId = employeeId;
      Console.WriteLine($"!!!!!-+++++++++-------- record ID {payrollRecord.PayrollRun.PayrollRunNumber}");
      currentPayRun.Records.Add(payrollRecord);
      //save changes to db
      await _payrollRunRepo.UpdateRun(currentPayRun);
    }
    
    public async Task AddRecordsCollectionToRunAsync(IList<PayrollRecord> recordsCollection)
    {
      var payperiod = await _payrollPeriodService.GetLastPeriodAsync();
      if (payperiod == null)
        throw new InvalidDataException("No payroll period found or it is locked");

      var currentPayRun = payperiod.Runs.Where(r => !r.IsLocked).OrderByDescending(r => r.PayrollRunNumber).FirstOrDefault();

      if (currentPayRun == null)
        throw new InvalidDataException("No current payroll run found or it is locked");
      
      //indexer
      int i = 0;
      
      foreach (var record in recordsCollection)
      {
        record.PayrollRun = currentPayRun;
        //record.EmployeeId = employeeId;
        currentPayRun.Records.Add(record);
        i++;
      }
      // currentPayRun.Records.List<PayrollRecord>.AddRange(recordsCollection);
      await _payrollRunRepo.UpdateRun(currentPayRun);
    }
    
    public async Task UpdateRunAsync(PayrollRun payrollRun)
    {
      await _payrollRunRepo.UpdateRun(payrollRun);
    }

    public async Task<PayrollRun> GetAllPayRecordsFromPayRunAsync(int payrollRunNumber)
    {

      var currentPeriod = await _payrollPeriodService.GetLastPeriodAsync();
      if (currentPeriod == null)
        throw new InvalidDataException("No payroll period found");

      var run = currentPeriod.Runs
             .FirstOrDefault(r => r.PayrollRunNumber == payrollRunNumber);
      if (run == null)
        throw new InvalidDataException("No payroll run found");

      return await _payrollRunRepo.GetAllPayRecordsFromPayRun(run);
    }

    public async Task LockAllOlderPayrollRuns()
    {
      int found = 0;
      PayrollRun? expiredRun;// = await _payrollRunRepo.IsExpiredPayRunUnlocked();
      while ((expiredRun = await _payrollRunRepo.IsExpiredPayRunUnlocked()) != null)
      {
        Console.WriteLine($"NUMBER OF OPEN EXPIRED RURNS {found++}");
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