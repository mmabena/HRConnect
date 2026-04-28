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
    ///</returns>Emplo
    ///<exception cref="NotFoundException"></exception>
    public async Task<EmployeePayrollEarningDto> AddAsync(EmployeePayrollEarningAddDto employeePayrollEarningAddDto)
    {
      ValidateEmployeePayrollEarningsDto.ValidateEmployeePayrollEarningAddDto(employeePayrollEarningAddDto);
      PayrollRun currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      EmployeePayrollEarning? existingEmployeePayrollEarning = await _employeePayrollEarningRepository.CheckIfEmployeeEarningExistsForCurrentPayrun
        (employeePayrollEarningAddDto.EmployeeId, employeePayrollEarningAddDto.PayrollEarningId, currentPayrollRun.PayrollRunId);
      PayrollEarning? payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(employeePayrollEarningAddDto.PayrollEarningId)
        ?? throw new NotFoundException($"Payroll earning with id {employeePayrollEarningAddDto.PayrollEarningId} can not be found");

      if (existingEmployeePayrollEarning == null && payrollEarning != null)
      {
        EmployeePayrollEarning newEmployeeEarning = employeePayrollEarningAddDto.ToEmployeePayrollEarningModel();
        newEmployeeEarning.TaxCode = payrollEarning.TaxCode;
        newEmployeeEarning.PayrollRunId = currentPayrollRun.PayrollRunId;
        newEmployeeEarning.IsLocked = false;
        decimal[] employeeEarningAmounts = await CalculateAmountForPayrollEarning(payrollEarning, employeePayrollEarningAddDto.EmployeeId,
          employeePayrollEarningAddDto.OverTimeHoursWorked, employeePayrollEarningAddDto.Amount);
        newEmployeeEarning.Amount = employeeEarningAmounts[0];
        newEmployeeEarning.CalculatedAmountAfterTax = employeeEarningAmounts[1];

        EmployeePayrollEarning addedEmployeePayrollEarning = await _employeePayrollEarningRepository.AddAsync(newEmployeeEarning);
        return addedEmployeePayrollEarning.ToEmployeePayrollEarningDto();
      }
      else
      {
        EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto = new()
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

    ///<summary>
    ///Intialized task to enroll employee to salary payroll earning
    ///</summary>
    ///<exception cref="NotFoundException"></exception>
    public async Task InitializeEmployeePayrollEarningsAsync()
    {
      PayrollRun? currentPayRollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> existingEmployees = await _employeeRepository.GetAllEmployeesAsync();
      PayrollEarning salaryPayrollEarning = await _payrollEarningRepository.GetByPayrollEarningId("PRE001") ?? throw new NotFoundException("Salary payroll earning not found");
      List<EmployeePayrollEarning> employeeSalaryPayrollEarnings = [];

      foreach (Employee employee in existingEmployees)
      {
        List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByEmployeeIdAsync(employee.EmployeeId);

        if (employeePayrollEarnings.Any(e => e.PayrollEarningId == "PRE001"))
        {
          continue;
        }

        EmployeePayrollEarning employeeSalaryPayrollEarning = new()
        {
          EmployeeId = employee.EmployeeId,
          PayrollEarningId = salaryPayrollEarning.PayrollEarningId,
          TaxCode = salaryPayrollEarning.TaxCode,
          OverTimeHoursWorked = null,
          PayrollRunId = currentPayRollRun.PayrollRunId,
          IsLocked = false
        };


        employeeSalaryPayrollEarning.Amount = employee.MonthlySalary;
        int employeeAge = CalculateAge.UsingDOB(employee.DateOfBirth);
        decimal tax = await _taxDeductionService.CalculateTaxAsync(employee.MonthlySalary, employeeAge);
        employeeSalaryPayrollEarning.CalculatedAmountAfterTax = employee.MonthlySalary - tax;

        employeeSalaryPayrollEarnings.Add(employeeSalaryPayrollEarning);
      }

      try
      {
        await _employeePayrollEarningRepository.AddRangeAsync(employeeSalaryPayrollEarnings);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error locking employee payroll earnings: {ex}");
      }
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
    ///Roll over employee payroll earnings for employees
    ///</summary>
    public async Task RollOverEmployeePayrollEarningsAsync()
    {
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> employees = await _employeeRepository.GetAllEmployeesAsync();

      foreach (Employee employee in employees)
      {
        List<EmployeePayrollEarning> employeeExistingPayrollEarnings = await _employeePayrollEarningRepository.GetByEmployeeIdAndLastRunIdAsync(employee.EmployeeId);
        List<EmployeePayrollEarning> employeePayrollEarningToBeRolledOver = [];
        foreach (EmployeePayrollEarning employeePayrollEarning in employeeExistingPayrollEarnings)
        {
          PayrollEarning? payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(employeePayrollEarning.PayrollEarningId)
             ?? throw new NotFoundException($"Payroll earning with id {employeePayrollEarning.PayrollEarningId} can not be found");

          EmployeePayrollEarning newEmployeePayrollEarning = new()
          {
            EmployeeId = employee.EmployeeId,
            PayrollEarningId = employeePayrollEarning.PayrollEarningId,
            TaxCode = employeePayrollEarning.TaxCode,
            IsLocked = false,
            PayrollRunId = currentPayrollRun.PayrollRunId
          };

          if (payrollEarning.TaxCode == 3601 &&
          payrollEarning.OvertimeHourMultiplier == null &&
          newEmployeePayrollEarning.OverTimeHoursWorked == null &&
          payrollEarning.CanProRata)
          {
            newEmployeePayrollEarning.Amount = employee.MonthlySalary;
            int employeeAge = CalculateAge.UsingDOB(employee.DateOfBirth);
            decimal tax = await _taxDeductionService.CalculateTaxAsync(employee.MonthlySalary, employeeAge);
            newEmployeePayrollEarning.CalculatedAmountAfterTax = employee.MonthlySalary - tax;
          }
          else
          {
            newEmployeePayrollEarning.Amount = 0m;
            newEmployeePayrollEarning.CalculatedAmountAfterTax = 0m;
          }

          if (employeePayrollEarning.PayrollRunId == newEmployeePayrollEarning.PayrollRunId)
          {
            continue;
          }

          employeePayrollEarningToBeRolledOver.Add(newEmployeePayrollEarning);
        }

        await _employeePayrollEarningRepository.AddRangeAsync(employeePayrollEarningToBeRolledOver);
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
      PayrollEarning? payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(employeePayrollEarningUpdateDto.PayrollEarningId)
        ?? throw new NotFoundException($"Payroll earning with id {employeePayrollEarningUpdateDto.PayrollEarningId} can not be found");
      EmployeePayrollEarning? existingEmployeePayrollEarning = await _employeePayrollEarningRepository.CheckIfEmployeeEarningExistsForCurrentPayrun
        (employeePayrollEarningUpdateDto.EmployeeId, employeePayrollEarningUpdateDto.PayrollEarningId, currentPayrollRun.PayrollRunId)
        ?? throw new NotFoundException($"Employee payroll earning not found for employee id {employeePayrollEarningUpdateDto.EmployeeId} " +
        $"and payroll earning id {employeePayrollEarningUpdateDto.PayrollEarningId}");

      existingEmployeePayrollEarning.OverTimeHoursWorked = employeePayrollEarningUpdateDto.OverTimeHoursWorked ?? existingEmployeePayrollEarning.OverTimeHoursWorked;
      decimal[] employeePayrollAmounts = await CalculateAmountForPayrollEarning(payrollEarning, existingEmployeePayrollEarning.EmployeeId, employeePayrollEarningUpdateDto.OverTimeHoursWorked,
        employeePayrollEarningUpdateDto.Amount);
      existingEmployeePayrollEarning.Amount = employeePayrollAmounts[0];
      existingEmployeePayrollEarning.CalculatedAmountAfterTax = employeePayrollAmounts[1];

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
    private async Task<decimal[]> CalculateAmountForPayrollEarning(PayrollEarning payrollEarning, string employeeId, int? OverTimeHoursWorked, decimal? Amount)
    {
      Employee employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId)
        ?? throw new NotFoundException($"Employee with id {employeeId} not found");
      int age = CalculateAge.UsingDOB(employee.DateOfBirth);
      if (payrollEarning.IsActive)
      {
        decimal workingDays = 21.67m;
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
            tax = await _taxDeductionService.CalculateTaxAsync(proRataSalary, age);
            taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);
          }

          return [proRataSalary, proRataSalary - taxableAmount];
        }
        else if (payrollEarning.TaxCode == 3601
          && payrollEarning.OvertimeHourMultiplier.HasValue
          && OverTimeHoursWorked.HasValue) //Over time 
        {
          decimal overtimeEarnings = Math.Round(employeeSalaryHourlyRate * payrollEarning.OvertimeHourMultiplier.Value * OverTimeHoursWorked.Value, 2);
          if (payrollEarning.Taxable && payrollEarning.TaxPercentage != null)
          {
            tax = await _taxDeductionService.CalculateTaxAsync(overtimeEarnings, age);
            taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);
          }

          return [overtimeEarnings, overtimeEarnings - taxableAmount];
        }
        else
        {
          if (payrollEarning.Taxable && payrollEarning.TaxPercentage != null && Amount.HasValue)
          {
            tax = await _taxDeductionService.CalculateTaxAsync((decimal)Amount, age);
            taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);

            return [(decimal)Amount, (decimal)Amount - taxableAmount];
          }
          else
          {
            return (Amount != null) ? [(decimal)Amount, (decimal)Amount] : [0, 0];
          }
        }
      }
      else
      {
        //throw new InvalidOperationException($"Payroll earning with id {payrollEarning.PayrollEarningId} is not active");
        return [0, 0];
      }
    }
  }
}
