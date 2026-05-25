#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
namespace HRConnect.Api.Services;

using HRConnect.Api.DTOs.MedicalOption;
using HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Mappers;
using HRConnect.Api.Models.Payroll;
using HRConnect.Api.Models.PayrollDeduction;
using HRConnect.Api.Utils.MedicalAidDeduction;

/// <summary>
/// Service implementation for managing medical aid deductions.
/// </summary>
public class MedicalAidDeductionService : IMedicalAidDeductionService
{
  private readonly IMedicalAidDeductionRepository _medicalAidDeductionRepository;
  private readonly IMedicalOptionRepository _medicalOptionRepository;
  private readonly IPayrollRunService _payrollRunService;
  private readonly IEmployeeService _employeeService;
  private readonly IMedicalAidEligibilityService _medicalAidEligibilityService;
  private readonly IServiceScopeFactory _serviceScopeFactory;

  public MedicalAidDeductionService(
      IMedicalAidDeductionRepository medicalAidDeductionRepository,
      IMedicalOptionRepository medicalOptionRepository,
      IEmployeeService employeeService, IPayrollRunService payrollRunService, 
      IMedicalAidEligibilityService medicalAidEligibilityService,
      IServiceScopeFactory serviceScopeFactory)
  {
    _medicalAidDeductionRepository = medicalAidDeductionRepository;
    _medicalOptionRepository = medicalOptionRepository;
    _employeeService = employeeService;
    _payrollRunService = payrollRunService;
    _medicalAidEligibilityService = medicalAidEligibilityService;
    _serviceScopeFactory = serviceScopeFactory;
  }



  public async Task<MedicalAidDeductionDto> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId)
  {
    var employeeDeductions = await _medicalAidDeductionRepository
        .GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

    if (employeeDeductions == null || employeeDeductions.Count == 0)
    {
      throw new KeyNotFoundException($"No medical aid deductions found for employee {employeeId}");
    }

    // Return the first/most recent deduction
    var deduction = employeeDeductions.First();
    return MedicalAidDeductionMapper.MapToDto(deduction);
  }

  public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductions()
  {
    return await _medicalAidDeductionRepository.GetAllMedicalAidDeductionsAsync();
  }

  public async Task<MedicalAidDeductionDto> AddNewMedicalAidDeductions(string employeeId,
      int medicalOptionId,
      CreateMedicalAidDeductionRequestDto request)
  {
    // Get employee details
    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
    if (employee == null)
    {
      throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
    }        
    
    if (employee.EmploymentStatus.ToString() != "Permanent")
      throw new ArgumentException("Medical Aid is only applicable to permanent employees");
        
        // Get medical option details to ensure it exists and get category info
    var medicalOption = await _medicalOptionRepository.GetMedicalOptionByIdAsync(medicalOptionId);

    //get category information
    var category =
      await _medicalOptionRepository.GetCategoryByIdAsync(medicalOption!.MedicalOptionCategoryId);

    //check for dups
    var dupFound = await _medicalAidDeductionRepository.GetActiveMedicalAidDeductionByEmpIdAsync(employeeId);

    if (dupFound != null)
      throw new ArgumentException("Employee already has an active medical aid deduction");

    // Get Category Premium Ratings
    decimal? principalPremium = null;
    decimal? adultPremium = null;
    decimal? spousePremium = null;
    decimal? childPremium = null;
    decimal? child2Premium = null;
    decimal? totalPrincipalPremium = null; //principal member contribution
    decimal? totalAdultPremium = null;
    decimal? totalChildPremium = null;

    switch (category?.MedicalOptionCategoryName)
    {
      case "Network Choice":
      case "First Choice":
        if (medicalOption.MedicalOptionName.ToString().Contains("Network"))
        {
          // Get the base premium rates
          //Principal adult, child and child2 (free - applicable from variant 1 -3 )
          principalPremium = medicalOption.MonthlyRiskContributionPrincipal;
          adultPremium = medicalOption.MonthlyRiskContributionAdult;
          childPremium = medicalOption.MonthlyRiskContributionChild;
          child2Premium = medicalOption.MonthlyRiskContributionChild2;

          if (char.IsDigit(medicalOption.MedicalOptionName[^1]))
          {
            childPremium = medicalOption.MonthlyRiskContributionChild;
            child2Premium = medicalOption.MonthlyRiskContributionChild2 ?? 0;
          }
          else
          {
            //else if variant 4+, then consider child2+ == child1
            childPremium = medicalOption.MonthlyRiskContributionChild;
            child2Premium = childPremium;
          }
        }
        else if (medicalOption.MedicalOptionName.ToString().Contains("First"))
        {
          //No Principal and Child2
          principalPremium = 0;
          adultPremium = medicalOption.MonthlyRiskContributionAdult;
          childPremium = medicalOption.MonthlyRiskContributionChild;
          child2Premium = 0;
        }
        break;

      case "Essential":
        // MSA + Risk + Principal
        principalPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionPrincipal! +
                                (decimal)medicalOption.MonthlyRiskContributionPrincipal!);
        adultPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionAdult! +
                               (decimal)medicalOption.MonthlyRiskContributionAdult!);
        childPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionChild! +
                               (decimal)medicalOption.MonthlyRiskContributionChild!);
        child2Premium = 0;
        break;

      case "Vital":
        //Risk only and No Principal
        principalPremium = 0;
        adultPremium = medicalOption.MonthlyRiskContributionAdult;
        childPremium = medicalOption.MonthlyRiskContributionChild;
        child2Premium = 0;
        break;

      case "Double":
        //MSA + Risk | No Principal and Child2
        principalPremium = 0;
        adultPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionAdult! +
                                      (decimal)medicalOption.MonthlyRiskContributionAdult!);
        childPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionChild! +
                                      (decimal)medicalOption.MonthlyRiskContributionChild!);
        break;

      case "Alliance":
        //MAS + Risk | No Principal and Child2
        principalPremium = 0;
        adultPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionAdult! +
                                      (decimal)medicalOption.MonthlyRiskContributionAdult!);
        childPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionChild! +
                                      (decimal)medicalOption.MonthlyRiskContributionChild!);
        child2Premium = 0;
        break;

      default:
        //Calculate
        principalPremium = Math.Abs(((decimal)medicalOption?.MonthlyMsaContributionPrincipal! == null ?
                (decimal)medicalOption!.MonthlyMsaContributionAdult! : (decimal)medicalOption.MonthlyMsaContributionPrincipal) + 
                ((decimal)medicalOption!.MonthlyRiskContributionPrincipal! == null ? (decimal)medicalOption!.MonthlyMsaContributionAdult! :
                (decimal)medicalOption.MonthlyRiskContributionPrincipal));
        adultPremium = Math.Abs(((decimal)medicalOption!.MonthlyMsaContributionAdult! == null ? 0 : (decimal)medicalOption.MonthlyMsaContributionAdult)
        + (decimal)medicalOption.MonthlyRiskContributionAdult!);
        childPremium = Math.Abs(((decimal)medicalOption.MonthlyMsaContributionChild! == null ? 0 : (decimal)medicalOption.MonthlyMsaContributionChild)
          + (decimal)medicalOption!.MonthlyRiskContributionChild!);
        child2Premium = 0;
        break;
    }

    if (medicalOption == null)
      throw new KeyNotFoundException($"Medical option with ID {medicalOptionId} not found");

    //calculate Estimated Deductions (this will be for the special case of Network Choice)
    if (category.MedicalOptionCategoryName == "Network Choice" &&
        medicalOption.MedicalOptionName.ToString().Contains("Network"))
    {
      //check variant | if 1 - 3 -> child2+ == free else charged
      if (medicalOption.MedicalOptionName.Last() >= 1 && 
        medicalOption.MedicalOptionName.Last() <= 3)
      {
        //apply the free child2+ condition
        if (request.ChildrenCount > 0)
        {
          totalChildPremium = childPremium;
        }
        else if(request.ChildrenCount == 0)
        {
          totalChildPremium = 0;
        }
      }
      else
      {
        // Variant lies between 4 and 5
        totalChildPremium = Math.Abs((decimal)childPremium! * request.ChildrenCount);
      }
    }
    decimal principalPremiumEstimate = MedicalAidDeductionUtil.CalculatePrincipalPremium(medicalOption);
    decimal spousePremiumEstimate =
          MedicalAidDeductionUtil.CalculateAdultPremium(medicalOption, request.AdultCount);
    decimal childPremiumEstimate =
          MedicalAidDeductionUtil.CalculateChildPremium(medicalOption, request.ChildrenCount);
    decimal totalPremiumEstimate = MedicalAidDeductionUtil.CalculateTotalPremium(principalPremiumEstimate,
          spousePremiumEstimate, childPremiumEstimate);

    if (employee.MonthlySalary < totalPremiumEstimate)
      throw new ArgumentException("Total Premium estimate exceeds monthly salary");

    // Check if employee is eligible (Reinforcing the API to prevent bypass)
    var isEligible = await _medicalAidEligibilityService.isEligibleAsync(employeeId,
      medicalOptionId, request.PrincipalCount, request.AdultCount, request.ChildrenCount);

    if (!isEligible)
    {
      throw new ArgumentException("Employee is not eligible for this medical option");
    }

    // Create the deduction entity
    var deduction = new MedicalAidDeduction
    {
      // Employee details from employee service
      Name = employee.Name,
      Surname = employee.Surname,
      Branch = employee.Branch.ToString(),
      Salary = employee.MonthlySalary,
      EmployeeStartDate = employee.StartDate.ToDateTime(TimeOnly.MinValue),

      // Medical option details
      MedicalOptionId = medicalOptionId,
      OptionName = medicalOption.MedicalOptionName,
      MedicalCategoryId = category.MedicalOptionCategoryId,
      OptionCategoryName = category.MedicalOptionCategoryName,

      // Dependent counts from request
      PrincipalCount = request.PrincipalCount,
      AdultCount = request.AdultCount,
      ChildrenCount = request.ChildrenCount,

      // Premium amounts from request (already calculated by client from eligible options)
      PrincipalPremium = principalPremiumEstimate,
      SpousePremium = spousePremiumEstimate,
      ChildPremium = childPremiumEstimate, // cater for network choice
      TotalDeductionAmount = totalPremiumEstimate,

      // Effective date (default to now if not specified)
      EffectiveDate = MedicalAidDeductionUtil.GetEffectiveDate(employee.StartDate.ToDateTime(TimeOnly.MinValue)),

      // Set as active by default
      IsActive = MedicalAidDeductionUtil.EffectDateBeforeMidMonth(employee.StartDate.ToDateTime(TimeOnly.MinValue)),
      CreatedDate = DateTime.Now.ToLocalTime(),
      UpdatedDate = DateTime.Now.ToLocalTime()
    };

    // Save to repository

    await _payrollRunService.AddRecordToCurrentRunAsync(deduction, employee.EmployeeId);

    await _medicalAidDeductionRepository.AddNewMedicalAidDeductionsAsync(deduction);

    return MedicalAidDeductionMapper.MapToDto(deduction);
  }

  public async Task<UpdateMedicalAidDeductionResponseDto> UpdateDeductionsByEmpIdAsync(
    string employeeId,
    UpdateMedicalAidDeductionRequestDto updatePayload)
  {
    
    // First validate requestPayload
    if(updatePayload == null)
      throw new ArgumentNullException(nameof(updatePayload), "Update request cannot be empty");

    if (updatePayload.MedicalOptionId <= 0)
      throw new ArgumentException(
        "Medical option ID must be a valid positive integer, and cannot be null");
    
    if (updatePayload.MedicalCategoryId <= 0 || updatePayload.MedicalCategoryId == null )
      throw new ArgumentException(
        "Medical category ID must be a valid positive integer, and cannot be null");
    if (string.IsNullOrEmpty(updatePayload.OptionName))
      throw new ArgumentException("Option name cannot be empty");

    if (string.IsNullOrEmpty(updatePayload.OptionCategory))
      throw new ArgumentException("Option category cannot be empty");
    
    if (updatePayload.PrincipalCount < 0 || updatePayload.AdultCount < 0 || updatePayload.ChildrenCount < 0)
      throw new ArgumentException(
        "Principal count, adult count, and children count must be non-negative");
    
    if (updatePayload.PrincipalCount > 1)
      throw new ArgumentException("Principal count cannot exceed 1");

    // Separate scopes => separate DbContext instances per parallel branch
      using var payrollRunScope = _serviceScopeFactory.CreateScope();
      using var employeeScope = _serviceScopeFactory.CreateScope();
      using var medicalAidDeductionScope = _serviceScopeFactory.CreateScope();
      using var medicalOptionScope = _serviceScopeFactory.CreateScope();
      // Split Medical Options queries into seperate scopes/context
      using var medicalOptionCategoryScope = _serviceScopeFactory.CreateScope();
    
      var payrollRunService = payrollRunScope.ServiceProvider.GetRequiredService<IPayrollRunService>();
      var employeeService = employeeScope.ServiceProvider.GetRequiredService<IEmployeeService>();
      var medicalAidDeductionsRepository =
        medicalAidDeductionScope.ServiceProvider.GetRequiredService<IMedicalAidDeductionRepository>();
      var medicalOptionService = medicalOptionScope.ServiceProvider.GetRequiredService<IMedicalOptionService>();
      var medicalOptionCategoryService =
        medicalOptionCategoryScope.ServiceProvider.GetRequiredService<IMedicalOptionService>();   
      
      var payrollTask = payrollRunService.GetCurrentRunAsync();
      var employeeTask = employeeService.GetEmployeeByIdAsync(employeeId);
      var medicalAidDeductionTask =
        medicalAidDeductionsRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);
      var medicalOptionTask = medicalOptionService.GetMedicalOptionByIdAsync(updatePayload.MedicalOptionId);
      var medicalOptionCategoryTask = medicalOptionCategoryService.GetCategoryById(updatePayload.MedicalCategoryId);
    
      await Task.WhenAll(
        payrollTask,
        employeeTask,
        medicalAidDeductionTask,
        medicalOptionTask,
        medicalOptionCategoryTask);
    
      var currentRun = await payrollTask;
      var employeeData = await employeeTask;
      var medicalAidDeductionsData = await medicalAidDeductionTask;
      var medicalOptionData = await medicalOptionTask;
      var medicalOptionCategoryData = await medicalOptionCategoryTask;
    
      if (currentRun == null)
        throw new InvalidOperationException("No active payroll run found.");
    
      if (employeeData == null)
        throw new ArgumentException("Employee not found.");
    
      if (medicalOptionData == null)
        throw new ArgumentException("Medical option not found.");
    
      if (medicalOptionCategoryData == null || medicalOptionCategoryData.Count == 0)
        throw new ArgumentException("Medical option category not found.");
    
      if (medicalOptionData.MedicalOptionCategoryId != updatePayload.MedicalCategoryId)
        throw new ArgumentException(
          "The selected medical option does not belong to the provided medical category.");
    
      if (medicalAidDeductionsData == null || medicalAidDeductionsData.Count == 0)
        throw new ArgumentException("Active medical aid deduction not found.");
    
      var activeDeductionForCurrentRun = medicalAidDeductionsData
        .FirstOrDefault(d => d.PayrollRunId == currentRun.PayrollRunId);
    
      if (activeDeductionForCurrentRun == null)
        throw new ArgumentException("No active medical aid deduction found for the current payroll run.");
    
      decimal principalPremium = MedicalAidDeductionUtil.CalculatePrincipalPremium(medicalOptionData);
      decimal spousePremium = MedicalAidDeductionUtil.CalculateAdultPremium(medicalOptionData, updatePayload.AdultCount);
      decimal childPremium = MedicalAidDeductionUtil.CalculateChildPremium(medicalOptionData, updatePayload.ChildrenCount);
      decimal totalDeductionAmount = MedicalAidDeductionUtil.CalculateTotalPremium(principalPremium, spousePremium, childPremium);
    
      // Check if Total Sum is not greater than salary
      if (totalDeductionAmount > employeeData.MonthlySalary)
        throw new InvalidOperationException(
          "Update failed : Total Premium contributions must not exceed salary amount");
      
    var optionCategoryName = medicalOptionCategoryData[0].MedicalOptionCategoryName;

    var updateEntity = new MedicalAidDeduction
    {
      // preserve identity and immutable audit fields from existing record
      Id = activeDeductionForCurrentRun.Id,
      EmployeeId = activeDeductionForCurrentRun.EmployeeId,
      PayrollRunId = activeDeductionForCurrentRun.PayrollRunId,
      CreatedDate = activeDeductionForCurrentRun.CreatedDate,
      IsActive = activeDeductionForCurrentRun.IsActive,
      EffectiveDate = activeDeductionForCurrentRun.EffectiveDate,
      TerminationDate = activeDeductionForCurrentRun.TerminationDate,
      TerminationReason = activeDeductionForCurrentRun.TerminationReason,

      // refresh snapshot fields
      Name = employeeData.Name,
      Surname = employeeData.Surname,
      Branch = employeeData.Branch.ToString(),
      Salary = employeeData.MonthlySalary,
      EmployeeStartDate = employeeData.StartDate.ToDateTime(TimeOnly.MinValue),

      // option + category
      MedicalOptionId = medicalOptionData.MedicalOptionId,
      OptionName = medicalOptionData.MedicalOptionName,
      MedicalCategoryId = medicalOptionData.MedicalOptionCategoryId,
      OptionCategoryName = optionCategoryName,

      // dependent counts
      PrincipalCount = updatePayload.PrincipalCount,
      AdultCount = updatePayload.AdultCount,
      ChildrenCount = updatePayload.ChildrenCount,

      // premiums
      PrincipalPremium = principalPremium,
      SpousePremium = spousePremium,
      ChildPremium = childPremium,
      TotalDeductionAmount = totalDeductionAmount,

      UpdatedDate = DateTime.Now.ToLocalTime()
    };

    await medicalAidDeductionsRepository.UpdateDeductionsByEmpIdAsync(
      employeeId,
      currentRun.PayrollRunId,
      updateEntity);

    return MedicalAidDeductionMapper.ToUpdateMedicalAidDeductionResponseDto(updateEntity);

  }

  public async Task<TerminateMedicalAidDeductionResponseDto> TerminateDeductionsByEmpIdAsync(string employeeId,
    TerminateMedicalAidDeductionRequestDto terminationRequest)
  {
    if (string.IsNullOrWhiteSpace(employeeId))
      throw new ArgumentException("Employee ID is required.", nameof(employeeId));
    
    ArgumentNullException.ThrowIfNull(terminationRequest);

    if (terminationRequest.MedicalOptionId <= 0)
      throw new ArgumentException("Medical Option ID must be greater than 0 and positive.");
    
    if (string.IsNullOrWhiteSpace(terminationRequest.TerminationReason))
        throw new ArgumentException("Termination reason is required");

    var deductionEntity =
      await _medicalAidDeductionRepository.GetActiveMedicalAidDeductionByEmpIdAsync(employeeId);

    if (deductionEntity == null)
      throw new KeyNotFoundException(
        $"No active medical aid deduction found for employee '{employeeId}'.");
    if (deductionEntity.MedicalOptionId != terminationRequest.MedicalOptionId)
      throw new ArgumentException(
        $"Active deduction option ({deductionEntity.MedicalOptionId}) does not match request option" +
        $" ({terminationRequest.MedicalOptionId}).");
    
    // Snapshot values before reset
    var terminationResponse = new TerminateMedicalAidDeductionResponseDto
    {
      Id = deductionEntity.Id,
      EmployeeId = deductionEntity.EmployeeId,
      MedicalOptionId = deductionEntity.MedicalOptionId,
      OptionName = deductionEntity.OptionName,
      // before termination
      PreviousPrincipalCount = deductionEntity.PrincipalCount,
      PreviousAdultCount = deductionEntity.AdultCount,
      PreviousChildrenCount = deductionEntity.ChildrenCount,

      PreviousPrincipalPremium = deductionEntity.PrincipalPremium,
      PreviousSpousePremium = deductionEntity.SpousePremium,
      PreviousChildrenPremium = deductionEntity.ChildPremium,
      PreviousTotalDeductionAmount = deductionEntity.TotalDeductionAmount
    };
    
    // aftertermination response build up
    var now = DateTime.Now.ToLocalTime();
    var endOfMonth = new DateTime(
      now.Year,
      now.Month,
      DateTime.DaysInMonth(now.Year, now.Month),
      23, 59, 59,
      now.Kind);
    
    //Soft terminate and reset contributions
    deductionEntity.TerminationDate = endOfMonth;
    deductionEntity.TerminationReason = terminationRequest.TerminationReason.Trim();
    deductionEntity.IsActive = false;

    deductionEntity.PrincipalCount = 0;
    deductionEntity.AdultCount = 0;
    deductionEntity.ChildrenCount = 0;

    deductionEntity.PrincipalPremium = 0m;
    deductionEntity.SpousePremium = 0m;
    deductionEntity.ChildPremium = 0m;
    deductionEntity.TotalDeductionAmount = 0m;
    
    deductionEntity.UpdatedDate = now;

    await _medicalAidDeductionRepository.TerminateMedicalAidDeductionAsync(deductionEntity);

    terminationResponse.PrincipalCount = deductionEntity.PrincipalCount;
    terminationResponse.AdultCount = deductionEntity.AdultCount;
    terminationResponse.ChildrenCount = deductionEntity.ChildrenCount;
    terminationResponse.PrincipalPremium = deductionEntity.PrincipalPremium;
    terminationResponse.SpousePremium = deductionEntity.SpousePremium;
    terminationResponse.ChildPremium = deductionEntity.ChildPremium;
    terminationResponse.TotalDeductionAmount = deductionEntity.TotalDeductionAmount;
    terminationResponse.TerminationDate = deductionEntity.TerminationDate!.Value;
    terminationResponse.TerminationReason = deductionEntity.TerminationReason;
    terminationResponse.IsActive = deductionEntity.IsActive;
    terminationResponse.UpdatedDate = deductionEntity.UpdatedDate;

    return terminationResponse;

  }

  public async Task<List<MedicalAidDeduction>> GetAllRecordsFromPreviousRunAsync(int previousRunNumber)
  {
    return await _medicalAidDeductionRepository.GetAllRecordsFromPreviousRun(previousRunNumber);
  }

  public async Task RollOverMedicalAidDeductions()
  {
    int currentMonth = DateTime.Now.Month;
    int currentYear = DateTime.Now.Year;
    
    // Get all deductions from the previous run
    var currentRun = await _payrollRunService.GetCurrentRunAsync();
    if (currentRun == null)
      throw new InvalidDataException("No current payroll run found");
    
    var previousPayRunNumber = currentRun.PayrollRunId -1;

    var previousDeductions =
      await _medicalAidDeductionRepository.GetAllRecordsFromPreviousRun(previousPayRunNumber);

    if (previousDeductions == null || previousDeductions.Count == 0)
      throw new InvalidDataException("No previous Medical Aid Deductions found on the previous Run");
    
    //Roll over the deductions
    var filteredPreviousDeductions = previousDeductions
      .Where(p => (p.TerminationDate == null ||
                  (p.TerminationDate.Value.Month > currentMonth &&
                   p.TerminationDate.Value.Year >= currentYear)) &&
                  !p.IsActive
                  )
      .OrderBy(p => p.Id)
      .ToList();
    
    var recordsToRollover = new List<PayrollRecord>();
    var employeeIds = new List<string>();


    
    foreach (var previousDeduction in filteredPreviousDeductions)
    {
      // Create a new instance
      var newDeduction = new MedicalAidDeduction
      {
        EmployeeId = previousDeduction.EmployeeId,
        Name = previousDeduction.Name,
        Surname = previousDeduction.Surname,
        Branch = previousDeduction.Branch,
        Salary = previousDeduction.Salary,
        EmployeeStartDate = previousDeduction.EmployeeStartDate,
        EffectiveDate = previousDeduction.EffectiveDate,
        MedicalOptionId = previousDeduction.MedicalOptionId,
        MedicalCategoryId = previousDeduction.MedicalCategoryId,
        PrincipalCount = previousDeduction.PrincipalCount,
        AdultCount = previousDeduction.AdultCount,
        ChildrenCount = previousDeduction.ChildrenCount,
        PrincipalPremium = previousDeduction.PrincipalPremium,
        SpousePremium = previousDeduction.SpousePremium,
        ChildPremium = previousDeduction.ChildPremium,
        TotalDeductionAmount = previousDeduction.TotalDeductionAmount,
        IsActive = true,  // New records are active
        CreatedDate = DateTime.Now,
        TerminationDate = previousDeduction.TerminationDate,
        TerminationReason = previousDeduction.TerminationReason,
        UpdatedDate = previousDeduction.UpdatedDate,

        OptionName = previousDeduction.OptionName,
        OptionCategoryName = previousDeduction.OptionCategoryName,
        PayrollRunId = currentRun.PayrollRunId  // Set to NEW run ID
      };
      recordsToRollover.Add(newDeduction);
    }
    
    await _payrollRunService.AddRecordsCollectionToRunAsync(recordsToRollover);
    

    
    await SaveChangesAsync();
  }

  public async Task SaveChangesAsync()
  {
    await _medicalAidDeductionRepository.SaveChangesAsync();
  }

  public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllLockedMedicalAidDeductions()
  {
    return await _medicalAidDeductionRepository.GetAllMedicalAidDeductionsAsync();
  }
}