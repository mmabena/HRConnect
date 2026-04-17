namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Payroll.Earning;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Payroll.Earning;
  using HRConnect.Api.Mappers.Payroll.Earning;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.Payroll.Earning;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Utils.ValidationHelpers.PayrollEarning;

  public class EmployeePayrollEarningService(IEmployeePayrollEarningRepository employeePayrollEarningRepository,
    IPayrollRunRepository payrollRunRepository, IEmployeeRepository employeeRepository, IPayrollEarningRepository payrollEarningRepository,
    ITaxDeductionService taxDeductionService) : IEmployeePayrollEarningService
  {
    private readonly IEmployeePayrollEarningRepository _employeePayrollEarningRepository = employeePayrollEarningRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IPayrollEarningRepository _payrollEarningRepository = payrollEarningRepository;
    private readonly ITaxDeductionService _taxDeductionService = taxDeductionService;

    ///<summary>
    ///Add employee payroll earning to the database     
    ///</summary>
    ///<param name="employeePayrollEarningAddDto">Employee payroll earning add request data transfer object</param>
    ///<returns>
    ///Added employee payroll earning 
    ///</returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<EmployeePayrollEarningDto> AddAsync(EmployeePayrollEarningAddDto employeePayrollEarningAddDto)
    {
      ValidateEmployeePayrollEarningsDto.ValidateEmployeePayrollEarningAddDto(employeePayrollEarningAddDto);
      PayrollRun currentPayrollRun = _payrollRunRepository.GetCurrentRunAsync().Result ?? throw new NotFoundException("Current payroll run not found");
      EmployeePayrollEarning? existingEmployeePayrollEarning = await _employeePayrollEarningRepository.CheckIfEmployeeEarningExistsForCurrentPayrun
        (employeePayrollEarningAddDto.EmployeeId, employeePayrollEarningAddDto.PayrollEarningId, currentPayrollRun.PayrollRunId);
      PayrollEarning? payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(employeePayrollEarningAddDto.PayrollEarningId);

      if (existingEmployeePayrollEarning == null && payrollEarning != null)
      {
        EmployeePayrollEarning newEmployeeEarning = employeePayrollEarningAddDto.ToEmployeePayrollEarningModel();
        newEmployeeEarning.TaxCode = payrollEarning.TaxCode;
        newEmployeeEarning.PayrollRunId = currentPayrollRun.PayrollRunId;
        newEmployeeEarning.IsLocked = false;
        newEmployeeEarning.Amount = await CalculateAmountForPayrollEarning(employeePayrollEarningAddDto.PayrollEarningId, employeePayrollEarningAddDto.EmployeeId,
          employeePayrollEarningAddDto.OverTimeHoursWorked, employeePayrollEarningAddDto.Amount);

        EmployeePayrollEarning addedEmployeePayrollEarning = await _employeePayrollEarningRepository.AddAsync(newEmployeeEarning);
        return addedEmployeePayrollEarning.ToEmployeePayrollEarningDto();
      }
      else
      {
        EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto = new EmployeePayrollEarningUpdateDto()
        {
          EmployeeId = employeePayrollEarningAddDto.EmployeeId,
          PayrollEarningId = employeePayrollEarningAddDto.PayrollEarningId,
          OverTimeHoursWorked = employeePayrollEarningAddDto.OverTimeHoursWorked,
          Amount = employeePayrollEarningAddDto.Amount
        };

        return await UpdateAsync(employeePayrollEarningUpdateDto);
      }
    }

    ///<summary>
    ///Retrieves all employee payroll earnings from the database and maps them to a list of EmployeePayrollEarningDto objects.             
    ///</summary>
    ///<returns>
    ///A list of EmployeePayrollEarningDto objects representing all employee payroll earnings in the database.
    ///</returns>
    public async Task<List<EmployeePayrollEarningDto>> GetAllAsync()
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetAllAsync();
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieves all employee payroll earnings for a specific employee that are not locked from the database
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarningDto objects representing all employee payroll earnings for a specific employee that are not locked in the database. 
    ///</returns>
    public async Task<List<EmployeePayrollEarningDto>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByEmployeeIdAndIsNotLockedAsync(employeeId);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieves all employee payroll earnings for a specific employee that are associated with the last payroll run in the database
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarningDto objects for all employee payroll earnings for a specific employee that are associated with the last payroll run in the database.
    ///</returns>
    public async Task<List<EmployeePayrollEarningDto>> GetByEmployeeIdAndLastRunIdAsync(string employeeId)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByEmployeeIdAndLastRunIdAsync(employeeId);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieves all employee payroll earnings for a specific employee from the database, regardless of whether they are locked or associated with the last payroll run.
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarningDto objects for all employee payroll earnings for a specific employee from the database.
    ///</returns>
    public async Task<List<EmployeePayrollEarningDto>> GetByEmployeeIdAsync(string employeeId)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByEmployeeIdAsync(employeeId);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieves all employee payroll earnings for a specific payroll earning from the database
    ///</summary>
    /// <param name="payrollEarningId">Payroll Earning Id</param>
    /// <returns>
    ///A list of EmployeePayrollEarningDto objects for all employee payroll earnings for a specific payroll earning from the database.
    ///</returns>
    public async Task<List<EmployeePayrollEarningDto>> GetByPayrollEarningIdAsync(string payrollEarningId)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByPayrollEarningIdAsync(payrollEarningId);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieves all employee payroll earnings for a specific payroll run from the database
    ///</summary>
    ///<param name="payrollRunId">Pay roll run Id</param>
    ///<returns>
    ///A list of EmployeePayrollEarningDto objects for all employee payroll earnings for a specific payroll run from the database.
    ///</returns>
    public async Task<List<EmployeePayrollEarningDto>> GetByPayrollRunIdAsync(int payrollRunId)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByPayrollRunIdAsync(payrollRunId);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieves all employee payroll earnings for a specific tax code from the database
    ///</summary>
    ///<param name="taxCode">Tax Code</param>
    ///<returns>
    ///A list of EmployeePayrollEarningDto objects for all employee payroll earnings for a specific tax code from the database.
    /// </returns>
    public async Task<List<EmployeePayrollEarningDto>> GetByTaxCodeAsync(int taxCode)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByTaxCodeAsync(taxCode);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    /// <summary>
    ///Retrieves all employee payroll earnings for a specific employee that are not locked from the database.
    /// </summary>
    /// <param name="employeeId">Employee Id</param>
    /// <returns>
    ///A list of EmployeePayrollEarningDto objects for all employee payroll earnings for a specific employee that are not locked from the database.
    ///   </returns>
    public async Task<List<EmployeePayrollEarningDto>> GetEmployeePayrollEarningsNotLocked(string employeeId)
    {
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetEmployeePayrollEarningsNotLocked(employeeId);
      return employeePayrollEarnings.Select(epe => epe.ToEmployeePayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Lock employee payroll earnings for current payroll run
    ///</summary>
    public async Task LockEmployeePayrollEarningsAsync()
    {
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByPayrollRunIdAsync(currentPayrollRun.PayrollRunId);

      foreach (EmployeePayrollEarning employeePayrollEarning in employeePayrollEarnings)
      {
        employeePayrollEarning.IsLocked = true;
      }

      try
      {
        await _employeePayrollEarningRepository.LockEmployeePayrollEarningsAsync(employeePayrollEarnings);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error locking employee payroll earnings: {ex}");
      }
    }

    ///<summary>
    ///Roll over employee payroll earningst for employees
    ///</summary>
    public async Task RollOverEmployeePayrollEarningsAsync()
    {
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> employees = await _employeeRepository.GetAllEmployeesAsync();

      foreach (Employee employee in employees)
      {
        List<EmployeePayrollEarning> employeeExistingPayrollEarnings = await _employeePayrollEarningRepository.GetByEmployeeIdAndLastRunIdAsync(employee.EmployeeId);

        foreach (EmployeePayrollEarning employeePayrollEarning in employeeExistingPayrollEarnings)
        {
          EmployeePayrollEarning newEmployeePayrollEarning = new()
          {
            EmployeeId = employee.EmployeeId,
            PayrollEarningId = employeePayrollEarning.PayrollEarningId,
            TaxCode = employeePayrollEarning.TaxCode,
            Amount = ((employeePayrollEarning.TaxCode == 3601) && (employeePayrollEarning.OverTimeHoursWorked == null)) ? employee.MonthlySalary : 0,
            IsLocked = false,
            PayrollRunId = currentPayrollRun.PayrollRunId
          };

          if (employeePayrollEarning.PayrollRunId == newEmployeePayrollEarning.PayrollRunId)
          {
            continue;
          }

          _ = await _employeePayrollEarningRepository.AddAsync(newEmployeePayrollEarning);
        }
      }
    }

    ///<summary>
    ///Update employee payroll earning entity in the database  
    ///</summary>
    ///<param name="employeePayrollEarningUpdateDto">Employee payroll earning update request data transfer object</param>
    ///<returns>
    ///Updated employee payroll earning entity
    ///</returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<EmployeePayrollEarningDto> UpdateAsync(EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto)
    {
      ValidateEmployeePayrollEarningsDto.ValidateEmployeePayrollEarningUpdateDto(employeePayrollEarningUpdateDto);
      PayrollRun currentPayrollRun = _payrollRunRepository.GetCurrentRunAsync().Result ?? throw new NotFoundException("Current payroll run not found");
      EmployeePayrollEarning? existingEmployeePayrollEarning = await _employeePayrollEarningRepository.CheckIfEmployeeEarningExistsForCurrentPayrun
        (employeePayrollEarningUpdateDto.EmployeeId, employeePayrollEarningUpdateDto.PayrollEarningId, currentPayrollRun.PayrollRunId)
        ?? throw new NotFoundException($"Employee payroll earning not found for employee id {employeePayrollEarningUpdateDto.EmployeeId} " +
        $"and payroll earning id {employeePayrollEarningUpdateDto.PayrollEarningId}");

      existingEmployeePayrollEarning.OverTimeHoursWorked = employeePayrollEarningUpdateDto.OverTimeHoursWorked ?? existingEmployeePayrollEarning.OverTimeHoursWorked;
      existingEmployeePayrollEarning.Amount = await CalculateAmountForPayrollEarning(existingEmployeePayrollEarning.PayrollEarningId,
        employeePayrollEarningUpdateDto.EmployeeId, employeePayrollEarningUpdateDto.OverTimeHoursWorked, employeePayrollEarningUpdateDto.Amount);

      EmployeePayrollEarning toBeUpdatedEmployeePayrollEarning = await _employeePayrollEarningRepository.UpdateAsync(existingEmployeePayrollEarning);

      return toBeUpdatedEmployeePayrollEarning.ToEmployeePayrollEarningDto();
    }

    ///<summary>
    ///Calculates the amount for a given employee payroll earning based on the associated payroll earning's tax code and hourly rate (if applicable). 
    ///</summary>
    ///<param name="employeePayrollEarning"></param>
    ///<returns>
    ///The calculated amount for the given employee payroll earning.
    ///</returns>
    ///<exception cref="NotFoundException">Payroll earning not found</exception>
    private async Task<decimal> CalculateAmountForPayrollEarning(string payrollEarningId, string employeeId, int? OverTimeHoursWorked, decimal? Amount)
    {
      PayrollEarning? payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(payrollEarningId);
      Employee employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId)
        ?? throw new NotFoundException($"Employee with id {employeeId} not found");
      int age = CalculateAge.UsingDOB(employee.DateOfBirth);
      if (payrollEarning != null)
      {
        if (payrollEarning.IsActive)
        {
          int workingDays = WorkingDayCalculator.CountWorkingDaysForCurrentMonth();
          if (workingDays == 0)
          {
            throw new InvalidOperationException("No working days in current month.");
          }

          decimal employeeSalaryHourlyRate = Math.Round(employee.MonthlySalary / (workingDays * 8), 2);
          decimal taxableAmount = 0m;
          decimal tax;

          if (payrollEarning.TaxCode == 3601 &&
            payrollEarning.OvertimeHourMultiplier == null &&
            OverTimeHoursWorked == null &&
            payrollEarning.CanProRata) //Regular earning
          {
            int remainingWorkingDays = WorkingDayCalculator.CountRemainingWorkingDaysForCurrentMonth();
            decimal proRataSalary = remainingWorkingDays * 8 * employeeSalaryHourlyRate;
            if (payrollEarning.Taxable && (payrollEarning.TaxPercentage != null))
            {
              tax = await _taxDeductionService.CalculateTaxAsync(proRataSalary * 12, age);
              taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);
            }

            return proRataSalary - taxableAmount;
          }
          else if (payrollEarning.TaxCode == 3601
            && payrollEarning.OvertimeHourMultiplier.HasValue
            && OverTimeHoursWorked.HasValue) //Over time 
          {
            decimal overtimeEarnings = Math.Round(employeeSalaryHourlyRate * payrollEarning.OvertimeHourMultiplier.Value * OverTimeHoursWorked.Value, 2);
            if (payrollEarning.Taxable && payrollEarning.TaxPercentage != null)
            {
              tax = await _taxDeductionService.CalculateTaxAsync(overtimeEarnings /** 12*/, age);
              taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);
            }

            return overtimeEarnings - taxableAmount;
          }
          else
          {
            if (payrollEarning.Taxable && payrollEarning.TaxPercentage != null && Amount.HasValue)
            {
              tax = await _taxDeductionService.CalculateTaxAsync((decimal)Amount * 12, age);
              taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);

              return (decimal)Amount - taxableAmount;
            }
            else
            {
              return Amount ?? 0m;
            }
          }
        }
        else
        {
          throw new InvalidOperationException($"Payroll earning with id {payrollEarningId} is not active");
        }
      }
      else
      {
        throw new NotFoundException($"Payroll earning with id {payrollEarningId} not found");
      }
    }

  }
}
