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

  public class PayrollEarningService(IPayrollEarningRepository payrollEarningRepository,
    IEmployeePayrollEarningRepository employeePayrollEarningRepository, IPayrollRunRepository payrollRunRepository,
    IEmployeeRepository employeeRepository, ITaxDeductionService taxDeductionService) : IPayrollEarningService
  {
    private readonly IPayrollEarningRepository _payrollEarningRepository = payrollEarningRepository;
    private readonly IEmployeePayrollEarningRepository _employeePayrollEarningRepository = employeePayrollEarningRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly ITaxDeductionService _taxDeductionService = taxDeductionService;

    ///<summary>
    ///Add a new payroll earning to the system. Payroll earning codes are auto generated and cannot be user input. 
    ///</summary>
    ///<param name="payrollEarningAddDto">Pay roll earning add request data transfer object</param>
    ///<returns>
    ///Added payroll earning details as a PayrollEarningDto object.
    /// </returns>
    ///<exception cref="ArgumentException"></exception>
    public async Task<PayrollEarningDto> AddPayrollEarningAsync(PayrollEarningAddDto payrollEarningAddDto)
    {
      ValidatePayrollEarningsDto.ValidatePayrollEarningAddDto(payrollEarningAddDto);
      await CheckForSimilarDescriptions(payrollEarningAddDto.ShortDescription, payrollEarningAddDto.LongDescription);
      PayrollEarning newPayrollEarning = payrollEarningAddDto.ToPayrollEarningModel();
      if (string.IsNullOrEmpty(newPayrollEarning.PayrollEarningId))
      {
        List<string> existingpayrollEarningIds = await _payrollEarningRepository.GetAllPayrollEarningIdsAsync("PRE");
        string payrollEarningId = GenerateUnqiueCode.GenerateStringCode("PRE", existingpayrollEarningIds);
        newPayrollEarning.PayrollEarningId = payrollEarningId;
        PayrollEarning addedPayrollEarning = await _payrollEarningRepository.AddAsync(newPayrollEarning);
        return addedPayrollEarning.ToPayrollEarningDto();
      }
      else
      {
        throw new ArgumentException("Pay roll earning codes are auto generated and cannot be user input");
      }
    }

    ///<summary>
    ///Retrieve a list of all payroll earnings in the system.
    ///</summary>
    ///<returns>
    ///A list of payroll earnings as PayrollEarningDto objects.
    /// </returns>
    public async Task<List<PayrollEarningDto>> GetAllPayrollEarningsAsync()
    {
      List<PayrollEarning> payrollEarnings = await _payrollEarningRepository.GetAllAsync();
      return payrollEarnings.Select(pre => pre.ToPayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Retrieve details of a specific payroll earning using its unique identifier, payrollEarningId.  
    ///</summary>
    ///<param name="payrollEarningId">Payroll earning code</param>
    ///<returns></returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<PayrollEarningDto?> GetPayrollEarningByIdAsync(string payrollEarningId)
    {
      PayrollEarning payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(payrollEarningId)
        ?? throw new NotFoundException("Employee not found");
      return payrollEarning.ToPayrollEarningDto();
    }

    ///<summary>
    ///Retrieve a list of payroll earnings based on the specified tax code.
    ///</summary>
    ///<param name="taxCode">Tax code</param>
    ///<returns>
    ///A list of payroll earnings as PayrollEarningDto objects
    ///</returns>
    public async Task<List<PayrollEarningDto>> GetPayrollEarningByTaxCode(int taxCode)
    {
      List<PayrollEarning> payrollEarnings = await _payrollEarningRepository.GetByTaxCode(taxCode);
      return payrollEarnings.Select(pre => pre.ToPayrollEarningDto()).ToList();
    }

    ///<summary>
    ///Set a payroll earning to inactive status.
    ///</summary>
    ///<param name="payrollEarningId">Payroll earning code</param>
    ///<returns>
    ///A string indicating the result of the operation
    ///</returns>
    public async Task<string> SetPayrollEarningToInactiveAsync(string payrollEarningId)
    {
      return await _payrollEarningRepository.DeleteAsync(payrollEarningId);
    }

    ///<summary>
    ///Update an existing payroll earning.
    ///</summary>
    ///<param name="payrollEarningUpdateDto">The payroll earning update data request transfer object</param>
    ///<returns>
    ///The updated payroll earning as a PayrollEarningDto object
    ///</returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<PayrollEarningDto> UpdatePayrollEarningAsync(PayrollEarningUpdateDto payrollEarningUpdateDto)
    {
      ValidatePayrollEarningsDto.ValidatePayrollEarningUpdateDto(payrollEarningUpdateDto);
      await CheckForSimilarDescriptions(payrollEarningUpdateDto.ShortDescription, payrollEarningUpdateDto.LongDescription);
      PayrollEarning payrollEarning = await _payrollEarningRepository.GetByPayrollEarningId(payrollEarningUpdateDto.PayrollEarningId)
        ?? throw new NotFoundException("Pay roll earning not found");

      payrollEarning.ShortDescription = payrollEarningUpdateDto.ShortDescription ?? payrollEarning.ShortDescription;
      payrollEarning.LongDescription = payrollEarningUpdateDto.LongDescription ?? payrollEarning.LongDescription;
      payrollEarning.Taxable = payrollEarningUpdateDto.Taxable ?? payrollEarning.Taxable;
      payrollEarning.TaxCode = payrollEarningUpdateDto.TaxCode ?? payrollEarning.TaxCode;
      payrollEarning.TaxPercentage = payrollEarningUpdateDto.TaxPercentage ?? payrollEarning.TaxPercentage;
      payrollEarning.OvertimeHourMultiplier = payrollEarningUpdateDto.OvertimeHourMultiplier ?? payrollEarning.OvertimeHourMultiplier;
      payrollEarning.CanProRata = payrollEarningUpdateDto.CanProRata ?? payrollEarning.CanProRata;
      payrollEarning.IsOnGoing = payrollEarningUpdateDto.IsOnGoing ?? payrollEarning.IsOnGoing;
      payrollEarning.IsActive = payrollEarningUpdateDto.IsActive ?? payrollEarning.IsActive;

      PayrollEarning updatedPayrollEarning = await _payrollEarningRepository.UpdateAsync(payrollEarning);

      await HandlePayrollEarningUpdate(updatedPayrollEarning);

      return updatedPayrollEarning.ToPayrollEarningDto();
    }

    ///<summary>
    ///Auxilary method to check if the short description or long description of a payroll earning already exists in the database. I
    ///</summary>
    ///<param name="shortDescription">The short description of the payroll earning</param>
    ///<param name="longDescription">The long description of the payroll earning</param>
    ///<exception cref="ValidationException"></exception>
    private async Task CheckForSimilarDescriptions(string? shortDescription, string? longDescription)
    {
      bool descriptionExists = await _payrollEarningRepository.CheckIfDescriptionsExists(shortDescription ?? "", longDescription ?? "");
      if (descriptionExists)
      {
        throw new ValidationException("A payroll earning with the same short description and long description already exists");
      }
    }

    ///<summary>
    ///Auxilary method to update payroll earning for employees in the current pyaroll run 
    ///</summary>
    ///<param name="payrollEarning">Payroll run model</param>
    ///<exception cref="NotFoundException"></exception>
    private async Task HandlePayrollEarningUpdate(PayrollEarning payrollEarning)
    {
      PayrollRun? currentPayrollRun = await _payrollRunRepository.GetCurrentRunAsync() ?? throw new NotFoundException("Current payroll run not found");
      List<EmployeePayrollEarning> employeePayrollEarnings = await _employeePayrollEarningRepository.GetByPayrollRunIdAsync(currentPayrollRun.PayrollRunId);

      foreach (EmployeePayrollEarning employeePayrollEarning in employeePayrollEarnings)
      {
        Employee? employee = await _employeeRepository.GetEmployeeByIdAsync(employeePayrollEarning.EmployeeId);
        if (employee != null)
        {
          employeePayrollEarning.TaxCode = payrollEarning.TaxCode;
          decimal[] employeePayrollEarningAmounts = await CalculateAmountForPayrollEarning(payrollEarning, employee.EmployeeId, employeePayrollEarning.OverTimeHoursWorked,
            employeePayrollEarning.Amount);
          employeePayrollEarning.Amount = employeePayrollEarningAmounts[0];
          employeePayrollEarning.CalculatedAmountAfterTax = employeePayrollEarningAmounts[1];
        }
      }

      try
      {
        await _employeePayrollEarningRepository.UpdateRangeAsync(employeePayrollEarnings);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    ///<summary>
    ///Calculates the amount for a given employee payroll earning based on the associated payroll earning's tax code and hourly rate (if applicable). 
    ///</summary>
    ///<param name="employeePayrollEarning"></param>
    ///<returns>
    ///The calculated amount for the given employee payroll earning.
    ///</returns>
    ///<exception cref="NotFoundException">Payroll earning not found</exception>
    private async Task<decimal[]> CalculateAmountForPayrollEarning(PayrollEarning payrollEarning, string employeeId, int? OverTimeHoursWorked, decimal Amount)
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
          if (payrollEarning.Taxable && (payrollEarning.TaxPercentage != null))
          {
            tax = await _taxDeductionService.CalculateTaxAsync(Amount, age);
            taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);
          }

          return [Amount, Amount - taxableAmount];
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
          if (payrollEarning.Taxable && payrollEarning.TaxPercentage != null)
          {
            tax = await _taxDeductionService.CalculateTaxAsync(Amount, age);
            taxableAmount = tax * Math.Round((decimal)payrollEarning.TaxPercentage / 100, 2);

            return [Amount, Amount - taxableAmount];
          }
          else
          {
            return [Amount, Amount];
          }
        }
      }
      else
      {
        return [0, 0];
      }
    }
  }
}
