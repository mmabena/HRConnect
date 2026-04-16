namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.Payroll.Earnings;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Payroll.Earnings;
  using HRConnect.Api.Mappers.Payroll.Earnings;
  using HRConnect.Api.Models.Payroll.Earnings;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Utils.ValidationHelpers.PayrollEarnings;

  public class PayrollEarningService(IPayrollEarningRepository payrollEarningRepository) : IPayrollEarningService
  {
    private readonly IPayrollEarningRepository _payrollEarningRepository = payrollEarningRepository;

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
      return updatedPayrollEarning.ToPayrollEarningDto();
    }

    /// <summary>
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
  }
}
