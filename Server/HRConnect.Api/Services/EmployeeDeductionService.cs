namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Payroll.Deduction;
  using HRConnect.Api.Mappers.Payroll.Deduction;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Utils.ValidationHelpers.Deduction;

  public class EmployeeDeductionService(IEmployeeDeductionRepository employeeDeductionRepository, IDeductionRepository deductionRepository,
    IEmployeeRepository employeeRepository, IPayrollRunRepository payrollRunRepository) : IEmployeeDeductionService
  {
    private readonly IEmployeeDeductionRepository _employeeDeductionRepository = employeeDeductionRepository;
    private readonly IDeductionRepository _deductionRepository = deductionRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly decimal UIFSalaryCap = 17712m;
    private readonly decimal UIFDeductionCap = 177.12m;

    ///<summary>
    ///Add a new employee deduction to current payroll run
    ///</summary>
    ///<param name="employeeDeductionAddDto"></param>
    ///<returns></returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<EmployeeDeductionDto> AddAsync(EmployeeDeductionAddDto employeeDeductionAddDto)
    {
      ValidateEmployeeDeductionDto.ValidateEmployeeDeductionAddDto(employeeDeductionAddDto);
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      Employee existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeDeductionAddDto.EmployeeId) ?? throw new NotFoundException("Employee not found");
      EmployeeDeduction? existingEmployeeDeduction = await _employeeDeductionRepository.CheckIfEmployeeDeductionExistsForCurrentPayrun
        (employeeDeductionAddDto.EmployeeId, employeeDeductionAddDto.DeductionId, currentPayrollRun.PayrollRunId);
      Deduction? deduction = await _deductionRepository.GetDeductionByCodeAsync(employeeDeductionAddDto.DeductionId)
        ?? throw new NotFoundException("Deduction not found");

      ValidateDeductionInputType(deduction.InputType, employeeDeductionAddDto.AmountOrPercentage);
      if (deduction.InputType == DeductionInputType.Amount && (deduction.MinimumValue != null || deduction.MaximumValue != null))
      {
        ValidateAmount(deduction.MaximumValue, deduction.MinimumValue, employeeDeductionAddDto.AmountOrPercentage);
      }

      if (existingEmployeeDeduction == null)
      {
        EmployeeDeduction newEmployeeDeduction = employeeDeductionAddDto.ToEmployeeDeductionModel();
        newEmployeeDeduction.DeductionType = deduction.DeductionType;
        newEmployeeDeduction.DeductionInputType = deduction.InputType;
        newEmployeeDeduction.PayrollRunId = currentPayrollRun.PayrollRunId;
        newEmployeeDeduction.IsLocked = false;

        newEmployeeDeduction.AmountOrPercentage = deduction.DeductionType == "UIF" ? 1m : employeeDeductionAddDto.AmountOrPercentage;
        if (deduction.InputType == DeductionInputType.Amount)
        {
          newEmployeeDeduction.CalculatedDeductionAmount = employeeDeductionAddDto.AmountOrPercentage;
        }
        else if (deduction.InputType == DeductionInputType.Percentage)
        {
          newEmployeeDeduction.CalculatedDeductionAmount = (deduction.DeductionType == "UIF")
            ? CalculateUIF(existingEmployee.MonthlySalary)
            : Math.Round(existingEmployee.MonthlySalary * (employeeDeductionAddDto.AmountOrPercentage / 100));
        }

        EmployeeDeduction addedEmployeeDeduction = await _employeeDeductionRepository.AddAsync(newEmployeeDeduction);
        return addedEmployeeDeduction.ToEmployeeDeductionDto();
      }
      else
      {
        EmployeeDeductionUpdateDto employeeDeductionUpdateDto = new()
        {
          EmployeeId = employeeDeductionAddDto.EmployeeId,
          DeductionId = employeeDeductionAddDto.DeductionId,
          AmountOrPercentage = employeeDeductionAddDto.AmountOrPercentage
        };

        return await UpdateAsync(employeeDeductionUpdateDto);
      }
    }

    ///<summary>
    ///Retreive all employee deductions 
    ///</summary>
    ///<returns>
    ///A list of employee deductions
    ///</returns>
    public async Task<List<EmployeeDeductionDto>> GetAllAsync()
    {
      List<EmployeeDeduction> employeeDeductions = await _employeeDeductionRepository.GetAllAsync();
      return employeeDeductions.Select(ed => ed.ToEmployeeDeductionDto()).ToList();
    }

    ///<summary>
    ///Retreive all employee deductions with matching deductionId
    ///</summary>
    ///<param name="deductionId">Unique deduction code</param>
    ///<returns>
    ///A list of employee deductions with matching deductionId
    ///</returns>
    public async Task<List<EmployeeDeductionDto>> GetByDeductionIdAsync(string deductionId)
    {
      List<EmployeeDeduction> employeeDeductions = await _employeeDeductionRepository.GetByDeductionIdAsync(deductionId);
      return employeeDeductions.Select(ed => ed.ToEmployeeDeductionDto()).ToList();
    }

    ///<summary>
    ///Retreive all employee's deductions with matching employeeId and is not locked
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee's deductions with matching employeeId and not locked
    ///</returns>
    public async Task<List<EmployeeDeductionDto>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId)
    {
      List<EmployeeDeduction> employeeDeductions = await _employeeDeductionRepository.GetByEmployeeIdAndIsNotLockedAsync(employeeId);
      return employeeDeductions.Select(ed => ed.ToEmployeeDeductionDto()).ToList();
    }

    ///<summary>
    ///Retreive all employee's deductions with matching employeeId for latest payroll run
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee's deductions with matching employeeId
    ///</returns>
    public async Task<List<EmployeeDeductionDto>> GetByEmployeeIdAndLastRunIdAsync(string employeeId)
    {
      List<EmployeeDeduction> employeeDeductions = await _employeeDeductionRepository.GetByEmployeeIdAndLastRunIdAsync(employeeId);
      return employeeDeductions.Select(ed => ed.ToEmployeeDeductionDto()).ToList();
    }

    ///<summary>
    ///Retreive all employee's deductions with matching employeeId
    ///</summary>
    ///<param name="employeeId">Unique deduction code</param>
    ///<returns>
    ///A list of employee's deductions with matching employeeId
    ///</returns>
    public async Task<List<EmployeeDeductionDto>> GetByEmployeeIdAsync(string employeeId)
    {
      List<EmployeeDeduction> employeeDeductions = await _employeeDeductionRepository.GetByEmployeeIdAsync(employeeId);
      return employeeDeductions.Select(ed => ed.ToEmployeeDeductionDto()).ToList();
    }

    ///<summary>
    ///Retreive all employee deductions with matching payroll run id
    ///</summary>
    ///<param name="payrollRunId">Payroll run id</param>
    ///<returns>
    ///A list of employee deductions with matching payroll run id
    ///</returns>
    public async Task<List<EmployeeDeductionDto>> GetByPayrollRunIdAsync(int payrollRunId)
    {
      List<EmployeeDeduction> employeeDeductions = await _employeeDeductionRepository.GetByPayrollRunIdAsync(payrollRunId);
      return employeeDeductions.Select(ed => ed.ToEmployeeDeductionDto()).ToList();
    }

    ///<summary>
    ///Lock employee deductions for current payroll run
    ///</summary>
    ///<param name="employeeDeductions">List of employee deductions to be locked</param>
    public async Task LockEmployeeDeductionsAsync()
    {
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<EmployeeDeduction> employeeDeductionsForCurrentPayrollRun = await _employeeDeductionRepository.GetByPayrollRunIdAsync(currentPayrollRun.PayrollRunId);

      foreach (EmployeeDeduction employeeDeduction in employeeDeductionsForCurrentPayrollRun)
      {
        employeeDeduction.IsLocked = true;
      }

      try
      {
        await _employeeDeductionRepository.LockEmployeeDeductionsAsync(employeeDeductionsForCurrentPayrollRun);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error locking employee deduction: {ex}");
      }
    }

    ///<summary>
    ///Update employee deduction details of current payroll run
    ///</summary>
    ///<param name="employeeDeductionUpdateDto"></param>
    ///<returns>
    ///Updated employee deduction for current paroll run
    ///</returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<EmployeeDeductionDto> UpdateAsync(EmployeeDeductionUpdateDto employeeDeductionUpdateDto)
    {
      ValidateEmployeeDeductionDto.ValidateEmployeeDeductionUpdateDto(employeeDeductionUpdateDto);
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      Employee existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeDeductionUpdateDto.EmployeeId) ?? throw new NotFoundException("Employee not found");
      EmployeeDeduction? existingEmployeeDeduction = await _employeeDeductionRepository.CheckIfEmployeeDeductionExistsForCurrentPayrun
        (employeeDeductionUpdateDto.EmployeeId, employeeDeductionUpdateDto.DeductionId, currentPayrollRun.PayrollRunId)
        ?? throw new NotFoundException("Employee deduction not found");
      Deduction? deduction = await _deductionRepository.GetDeductionByCodeAsync(employeeDeductionUpdateDto.DeductionId)
        ?? throw new NotFoundException("Deduction not found");
      ValidateDeductionInputType(deduction.InputType, employeeDeductionUpdateDto.AmountOrPercentage);
      ValidateAmount(deduction.MaximumValue, deduction.MinimumValue, employeeDeductionUpdateDto.AmountOrPercentage);

      existingEmployeeDeduction.AmountOrPercentage = deduction.DeductionType == "UIF" ? 1m : employeeDeductionUpdateDto.AmountOrPercentage;

      if (deduction.InputType == DeductionInputType.Amount)
      {
        existingEmployeeDeduction.CalculatedDeductionAmount = employeeDeductionUpdateDto.AmountOrPercentage;
      }
      else if (deduction.InputType == DeductionInputType.Percentage)
      {
        existingEmployeeDeduction.CalculatedDeductionAmount = (deduction.DeductionType == "UIF")
          ? CalculateUIF(existingEmployee.MonthlySalary)
          : Math.Round(existingEmployee.MonthlySalary * (employeeDeductionUpdateDto.AmountOrPercentage / 100));
      }

      EmployeeDeduction toBeUpdatedEmployeeDeduction = await _employeeDeductionRepository.UpdateAsync(existingEmployeeDeduction);

      return toBeUpdatedEmployeeDeduction.ToEmployeeDeductionDto();
    }

    ///<summary>
    ///Validate that employee deduction percentage is between 0 and 100 
    ///</summary>
    ///<param name="deductionInputType">Deduction input type</param>
    ///<param name="amountOrPercentage">Deduction amount</param>
    ///<exception cref="ValidationException"></exception>
    private void ValidateDeductionInputType(DeductionInputType deductionInputType, decimal amountOrPercentage)
    {
      if (deductionInputType == DeductionInputType.Percentage && amountOrPercentage > 100)
      {
        throw new ValidationException("Percentage can only be more than zero and less than one hundred");
      }
    }

    ///<summary>
    ///Validate that employee deduction amount is not more than deduction maximum value and is not lower than deduction minimum value 
    ///</summary>
    ///<param name="MaximumValue">Deduction maximum value</param>
    ///<param name="MinimumValue">Deduction minimum value</param>
    ///<param name="Amount">Employee deduction amount</param>
    ///<exception cref="ValidationException"></exception>
    private void ValidateAmount(decimal? MaximumValue, decimal? MinimumValue, decimal Amount)
    {
      if (MaximumValue != null && Amount > MaximumValue)
      {
        throw new ValidationException("Amount exceeds the maximum value prohibited for this deduction");
      }

      if (MinimumValue != null && Amount < MinimumValue)
      {
        throw new ValidationException("Amound is lower than the minimum value allowed for this deduction");
      }
    }

    ///<summary>
    ///Auxilary function to determine employee UIF deduction 
    ///</summary>
    ///<param name="monthlySalary">Employee monthly salary</param>
    ///<returns>
    ///UIF deduction given employee's salary
    ///</returns>
    private decimal CalculateUIF(decimal monthlySalary)
    {
      return monthlySalary > UIFSalaryCap ? UIFDeductionCap : monthlySalary * 0.01m;
    }

    ///<summary>
    ///Roll over employee deductions for employees
    ///</summary>
    public async Task RollOverEmployeePayrollEarningsAsync()
    {
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<Employee> employees = await _employeeRepository.GetAllEmployeesAsync();

      foreach (Employee employee in employees)
      {
        if (!employee.IsActive)
        {
          continue;
        }

        List<EmployeeDeduction> employeeExistingDeductions = await _employeeDeductionRepository.GetByEmployeeIdAndLastRunIdAsync(employee.EmployeeId);
        List<EmployeeDeduction> employeeDeductionToBeRolledOver = [];
        foreach (EmployeeDeduction employeeDeduction in employeeExistingDeductions)
        {
          Deduction? deduction = await _deductionRepository.GetDeductionByCodeAsync(employeeDeduction.DeductionId) ?? throw new NotFoundException("Deduction does not exist");

          decimal CalculatedDeduction = 0m;

          if (deduction.InputType == DeductionInputType.Percentage && deduction.DeductionType == "UIF")
          {
            CalculatedDeduction = CalculateUIF(employee.MonthlySalary);
          }

          EmployeeDeduction newEmployeeDeduction = new()
          {
            EmployeeId = employee.EmployeeId,
            DeductionId = employeeDeduction.DeductionId,
            DeductionType = employeeDeduction.DeductionType,
            DeductionInputType = employeeDeduction.DeductionInputType,
            AmountOrPercentage = (CalculatedDeduction == 0) ? 0 : employeeDeduction.AmountOrPercentage,
            CalculatedDeductionAmount = CalculatedDeduction,
            PayrollRunId = currentPayrollRun.PayrollRunId,
            IsLocked = false
          };

          if (employeeDeduction.PayrollRunId == newEmployeeDeduction.PayrollRunId)
          {
            continue;
          }

          employeeDeductionToBeRolledOver.Add(newEmployeeDeduction);
        }

        await _employeeDeductionRepository.AddRangeAsync(employeeDeductionToBeRolledOver);
      }
    }

  }
}
