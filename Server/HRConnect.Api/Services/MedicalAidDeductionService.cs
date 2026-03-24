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

  public async Task<MedicalAidDeductionDto> UpdateDeductionsByEmpIdAsync(string employeeId,
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
    await medicalAidDeductionsRepository.UpdateDeductionsByEmpIdAsync(employeeId, currentRunId,
      updateEntity);

    return MapToDto(updateEntity);
  }
    //TODO :  Move to Mappers
    /// <summary>
    /// Maps a MedicalAidDeduction entity to a MedicalAidDeductionDto.
    /// </summary>
    private static MedicalAidDeductionDto MapToDto(MedicalAidDeduction request)
    {
        return new MedicalAidDeductionDto
        {
            MedicalAidDeductionId = request.Id,
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