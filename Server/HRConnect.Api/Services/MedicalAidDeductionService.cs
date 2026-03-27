namespace HRConnect.Api.Services;

using DTOs.MedicalOption;
using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using Interfaces;
using Models.PayrollDeduction;
using Utils.MedicalAidDeduction;

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
    return MapToDto(deduction);
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

        // Is employee permanent ?
        if (employee.EmploymentStatus.ToString() != "Permanent")
          throw new ArgumentException("Medical Aid is only applicable to permanent employees");
          // throw exception only permanet employees are eligable for medical aid deductions


        // Check if there is a medical aid deduction against employee (need to refine it further through including active payroll run)

        // temp implementation
        //var existingDedcutions =
        // await _medicalAidDeductionRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

        //if ((existingDedcutions != null  || existingDedcutions.Count > 0 )&& existingDedcutions.Any(d => d.IsActive)) throw new ArgumentException("Employee has an existing medical aid deduction");

        // Get medical option details to ensure it exists and get category info
        var medicalOption = await _medicalOptionRepository.GetMedicalOptionByIdAsync(medicalOptionId);

        //get category information
        var category =
          await _medicalOptionRepository.GetCategoryByIdAsync(medicalOption.MedicalOptionCategoryId);

        // Get Category Premium Ratings
        decimal? principalPremium = null;
        decimal? adultPremium = null;
        decimal? spousePremium = null;
        decimal? childPremium = null;
        decimal? child2Premium = null;
        decimal? totalPrincipalPremium = null; //principal member contribution
        decimal? totalAdultPremium = null;
        decimal? totalChildPremium = null;

        switch (category.MedicalOptionCategoryName)
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
              principalPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionPrincipal +
                                          (decimal)medicalOption.MonthlyRiskContributionPrincipal);
              adultPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionAdult +
                                      (decimal)medicalOption.MonthlyRiskContributionAdult);
              childPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionChild +
                                      (decimal)medicalOption.MonthlyRiskContributionChild);
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
              adultPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionAdult +
                                      (decimal)medicalOption.MonthlyRiskContributionAdult);
              childPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionChild +
                                      (decimal)medicalOption.MonthlyRiskContributionChild);
              break;

            case "Alliance":
              //MAS + Risk | No Principal and Child2
              principalPremium = 0;
              adultPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionAdult +
                                      (decimal)medicalOption.MonthlyRiskContributionAdult);
              childPremium = Math.Abs((decimal)medicalOption.MonthlyMsaContributionChild +
                                      (decimal)medicalOption.MonthlyRiskContributionChild);
              child2Premium = 0;
              break;

            default:
              throw new ArgumentException(
                $"Invalid medical option category: {category.MedicalOptionCategoryName}");
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
            totalChildPremium = Math.Abs((decimal)childPremium * request.ChildrenCount);
          }
        }

        // Getting estimated Contributions
        // Need to consider skipping Network choice
        decimal principalPremiumEstimate = CalculatePrincipalPremium(medicalOption);
        decimal spousePremiumEstimate =
          CalculateAdultPremium(medicalOption, request.AdultCount);
        decimal childPremiumEstimate =
          CalculateChildPremium(medicalOption, request.ChildrenCount); // cater for network choice
        decimal totalPremiumEstimate = CalculateTotalPremium(principalPremiumEstimate,
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
      EffectiveDate = GetEffectiveDate(employee.StartDate.ToDateTime(TimeOnly.MinValue)),

      // Set as active by default
      IsActive = MedicalAidDeductionUtil.EffectDateBeforeMidMonth(employee.StartDate.ToDateTime(TimeOnly.MinValue)),
      CreatedDate = DateTime.Now.ToLocalTime(),
      UpdatedDate = DateTime.Now.ToLocalTime()
        };

    // Save to repository

    await _payrollRunService.AddRecordToCurrentRunAsync(deduction, employee.EmployeeId);

    await _medicalAidDeductionRepository.AddNewMedicalAidDeductionsAsync(deduction);

    return MapToDto(deduction);
  }

  public async Task<UpdateMedicalAidDeductionResponseDto> UpdateDeductionsByEmpIdAsync(
    string employeeId,
    UpdateMedicalAidDeductionRequestDto updatePayload)
  {
    
    // First validate requestPayload
    if(updatePayload == null) 
      throw new ArgumentNullException(nameof(updatePayload), "Update request cannot be empty");

    if (updatePayload.MedicalOptionId == null || updatePayload.MedicalOptionId <= 0)
      throw new ArgumentException(
        "Medical option ID must be a valid positive integer, and cannot be null");
    
    if (updatePayload.MedicalCategoryId == null || updatePayload.MedicalCategoryId <= 0)
      throw new ArgumentException(
        "Medical category ID must be a valid positive integer, and cannot be null");
    if (updatePayload.OptionName.ToString().Trim().Length < 0)
      throw new ArgumentException("Option name cannot be empty");

    if (updatePayload.OptionCategory.ToString().Trim().Length < 0)
      throw new ArgumentException("Option category cannot be empty");
    
    if (updatePayload.PrincipalCount < 0 || updatePayload.AdultCount < 0 || updatePayload.ChildrenCount < 0)
      throw new ArgumentException(
        "Principal count, adult count, and children count must be non-negative");
    
    if (updatePayload.PrincipalCount > 1)
      throw new ArgumentException("Principal count cannot exceed 1");
    /*
    // Create separate scopes for parallel operations
    using var payrollRunScope = _serviceScopeFactory.CreateScope();
    using var employeeScope = _serviceScopeFactory.CreateScope();
    using var medicalAidDeductionScope = _serviceScopeFactory.CreateScope();
    using var medicalOptionScope = _serviceScopeFactory.CreateScope();

    
    // Attach required service methods to scoped services
    var payrollRunService =
      payrollRunScope.ServiceProvider.GetRequiredService<IPayrollRunService>();
    
    var employeeService = employeeScope.ServiceProvider.GetRequiredService<IEmployeeService>();
    
    var medicalAidDeductionsRepository = medicalAidDeductionScope.ServiceProvider
      .GetRequiredService<IMedicalAidDeductionRepository>();
    
    var medicalOptionService =
      medicalOptionScope.ServiceProvider.GetRequiredService<IMedicalOptionService>();

    
    // Call the desired methods, to execute in parallel
    // Note : goal -> Get Medical Option and confirm if there is an active deduction on employee
    var payrollTask = payrollRunService.GetCurrentRunAsync();
    var employeeTask = employeeService.GetEmployeeByIdAsync(employeeId);
    var medicalAidDeductionTask =
      medicalAidDeductionsRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);
    var medicalOptionTask =
      medicalOptionService.GetMedicalOptionByIdAsync(updatePayload.MedicalOptionId);
    var medicalOptionCategoryTask =
      medicalOptionService.GetCategoryById(updatePayload.MedicalCategoryId);
    
    // Create a task to wait for all tasks to complete
    await Task.WhenAll(payrollTask, employeeTask, medicalAidDeductionTask, medicalOptionTask,
      medicalOptionCategoryTask);
    
    // Access data from service calls
    var currentRunId = payrollTask.Result.PayrollRunId;
    var employeeData = employeeTask.Result;
    var medicalAidDeductionsData = medicalAidDeductionTask.Result;
    var medicalOptionData = medicalOptionTask.Result;
    var medicalOptionCategoryData = medicalOptionCategoryTask.Result;
    
    
    
    if(employeeData == null) throw new ArgumentException("Employee not found");
    if(medicalOptionData == null) throw new ArgumentException("Medical option not found");
    if (medicalOptionCategoryData == null || medicalOptionCategoryData.Count == 0)
      throw new ArgumentException("Medical option category not found");
    if (medicalAidDeductionsData == null)
      throw new ArgumentException("Active medical aid deduction not found");


    
    //perform calculations
    
    decimal principalPremium = CalculatePrincipalPremium(medicalOptionData);
    decimal spousePremium = CalculateAdultPremium(medicalOptionData, updatePayload.AdultCount);
    decimal childPremium = CalculateChildPremium(medicalOptionData, updatePayload.ChildrenCount);
    decimal totalDeductionAmount = CalculateTotalPremium(principalPremium,spousePremium,childPremium);
    
    //create entity

    var updateEntity = new MedicalAidDeduction
    {
      // Employee details from employee service
      Name = employeeData.Name,
      Surname = employeeData.Surname,
      Branch = employeeData.Branch.ToString(),
      Salary = employeeData.MonthlySalary,
      EmployeeStartDate = employeeData.StartDate.ToDateTime(TimeOnly.MinValue),

      // Medical option details
      MedicalOptionId = updatePayload.MedicalOptionId,
      OptionName = medicalOptionData.MedicalOptionName,
      MedicalCategoryId = medicalOptionData.MedicalOptionCategoryId,
      OptionCategoryName = medicalOptionCategoryData.Select(c => c.MedicalOptionCategoryName).ToString(),

      // Dependent counts from request
      PrincipalCount = updatePayload.PrincipalCount,
      AdultCount = updatePayload.AdultCount,
      ChildrenCount = updatePayload.ChildrenCount,

      // Premium amounts from request (already calculated by client from eligible options)
      PrincipalPremium = principalPremium,
      SpousePremium = spousePremium,
      ChildPremium = childPremium, // cater for network choice
      TotalDeductionAmount = totalDeductionAmount,
      // Effective date (default to now if not specified)
      EffectiveDate = GetEffectiveDate(employeeData.StartDate.ToDateTime(TimeOnly.MinValue)),

      // Set as active by default
      UpdatedDate = DateTime.Now.ToLocalTime()
    };
    
    //Update
    //await medicalAidDeductionsRepository.UpdateDeductionsByEmpIdAsync(employeeId, currentRunId, updateEntity);
    await _medicalAidDeductionRepository.UpdateDeductionsByEmpIdAsync(employeeId, currentRunId,
      updateEntity);
    return MapToDto(updateEntity);*/

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
    
      decimal principalPremium = CalculatePrincipalPremium(medicalOptionData);
      decimal spousePremium = CalculateAdultPremium(medicalOptionData, updatePayload.AdultCount);
      decimal childPremium = CalculateChildPremium(medicalOptionData, updatePayload.ChildrenCount);
      decimal totalDeductionAmount = CalculateTotalPremium(principalPremium, spousePremium, childPremium);
    
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

    return ToUpdateMedicalAidDeductionResponseDto(updateEntity);

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
    // get all deductions from the previous run
    
    throw new NotImplementedException();
  }

  //TODO :  Move to Mappers
    /// <summary>
    /// Maps a MedicalAidDeduction entity to a MedicalAidDeductionDto.
    /// </summary>
    private static MedicalAidDeductionDto MapToDto(MedicalAidDeduction request)
    {
        return new MedicalAidDeductionDto
        {
            PayrollRunId = request.Id,
            EmployeeId = request.EmployeeId ?? string.Empty,
            Name = request.Name,
            Surname = request.Surname,
            Branch = request.Branch,
            Salary = request.Salary,
            EmployeeStartDate = request.EmployeeStartDate,
            EffectiveDate = request.EffectiveDate,
            MedicalOptionId = request.MedicalOptionId,
            MedicalCategoryId = request.MedicalCategoryId,
            PrincipalCount = request.PrincipalCount,
            AdultCount = request.AdultCount,
            ChildrenCount = request.ChildrenCount,
            PrincipalPremium = request.PrincipalPremium,
            SpousePremium = request.SpousePremium,
            ChildPremium = request.ChildPremium,
            TotalDeductionAmount = request.TotalDeductionAmount,
            CreatedDate = request.CreatedDate,
            IsActive = request.IsActive,
            UpdatedDate = request.UpdatedDate
        };
    }
    
    private static UpdateMedicalAidDeductionResponseDto ToUpdateMedicalAidDeductionResponseDto(MedicalAidDeduction response)
    {
      return new UpdateMedicalAidDeductionResponseDto
      {
        Id = response.Id,
        PayrollRunId = response.Id,
        EmployeeId = response.EmployeeId ?? string.Empty,
        Name = response.Name,
        Surname = response.Surname,
        Branch = response.Branch,
        Salary = response.Salary,
        EmployeeStartDate = response.EmployeeStartDate,
        EffectiveDate = response.EffectiveDate,
        MedicalOptionId = response.MedicalOptionId,
        OptionName = response.OptionName,
        MedicalCategoryId = response.MedicalCategoryId,
        OptionCategoryName = response.OptionCategoryName,
        PrincipalCount = response.PrincipalCount,
        AdultCount = response.AdultCount,
        ChildrenCount = response.ChildrenCount,
        PrincipalPremium = response.PrincipalPremium,
        SpousePremium = response.SpousePremium,
        ChildPremium = response.ChildPremium,
        TotalDeductionAmount = response.TotalDeductionAmount,
        CreatedDate = response.CreatedDate,
        IsActive = response.IsActive,
        UpdatedDate = response.UpdatedDate,
        TerminationDate = response.TerminationDate,
        TerminationReason = response.TerminationReason,
      };
    }

    //private methods

    /// <summary>
    /// Calculates the principal premium based on total monthly contributions.
    /// </summary>
    private static decimal CalculatePrincipalPremium(MedicalOptionDto? option)
    {
      return option.TotalMonthlyContributionsPrincipal ?? option.TotalMonthlyContributionsAdult ;
    }

    /// <summary>
    /// Calculates the adult premium based on number of adults and per-adult contribution.
    /// </summary>
    private static decimal CalculateAdultPremium(MedicalOptionDto? option, int numberOfAdults)
    {
      if (numberOfAdults <= 0) return 0m;

      decimal adultContribution = option.TotalMonthlyContributionsAdult;
      return adultContribution * numberOfAdults;
    }

    /// <summary>
    /// Calculates the child premium based on number of children and per-child contribution.
    /// </summary>
    private static decimal CalculateChildPremium(MedicalOptionDto? option, int numberOfChildren)
    {
      if (numberOfChildren <= 0) return 0m;

      decimal childContribution = option.TotalMonthlyContributionsChild;
      if (option.MedicalOptionName.Contains("Network") &&
          (int.TryParse(option.MedicalOptionName[^1].ToString(), out int index) && index > 0 && index < 4))
      {
        return childContribution;
      }
      return childContribution * numberOfChildren;
    }

    private static decimal CalculateTotalPremium(decimal principalPremium, decimal adultPremium,
      decimal childPremium)
    {
      return Math.Abs(principalPremium + adultPremium + childPremium);
    }

    /// <summary>
    /// Get Effective date based on date supplied.
    /// </summary>
    /// <remarks>
    /// In accordance to the business rules, if the date is before mid-month, the effective date is the start date of the employee
    /// Else the effective date is the 1st of the following month within the current year.
    /// </remarks>
    /// 
    private static DateTime GetEffectiveDate(DateTime date)
    {
      return date.Day <= 15
        ? date
        : new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month, 1);
    }
}