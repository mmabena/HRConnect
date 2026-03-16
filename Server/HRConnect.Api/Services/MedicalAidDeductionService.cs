namespace HRConnect.Api.Services;

using DTOs.MedicalOption;
using DTOs.Employee;
using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using HRConnect.Api.Models;
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

  public MedicalAidDeductionService(
      IMedicalAidDeductionRepository medicalAidDeductionRepository,
      IMedicalOptionRepository medicalOptionRepository,
      IEmployeeService employeeService, IPayrollRunService payrollRunService)
  {
    _medicalAidDeductionRepository = medicalAidDeductionRepository;
    _medicalOptionRepository = medicalOptionRepository;
    _employeeService = employeeService;
    _payrollRunService = payrollRunService;

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
        var existingDedcutions =
         await _medicalAidDeductionRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

        //if (existingDeductions != null && existingDeductions.Any(d => d.IsActive) throw new ArgumentException("Employee has an existing medical aid deduction");

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
          CalculateAdultPremium(medicalOption, request.AdultCount, request);
        decimal childPremiumEstimate =
          CalculateChildPremium(medicalOption, request.ChildrenCount, request); // cater for network choice
        decimal totalPremiumEstimate = CalculateTotalPremium(principalPremiumEstimate,
          spousePremiumEstimate, childPremiumEstimate);

        if (employee.MonthlySalary < totalPremiumEstimate)
          throw new ArgumentException("Total Premium estimate exceeds monthly salary");

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
            EffectiveDate = MedicalAidDeductionUtil.EffectDateBeforeMidMonth(employee.StartDate.ToDateTime(TimeOnly.MinValue)) ? employee.StartDate.ToDateTime(TimeOnly.MinValue) : DateTime.Now.AddMonths(1).AddDays( -(1 - employee.StartDate.ToDateTime(TimeOnly.MinValue).Day) ),

            // Set as active by default
            IsActive = MedicalAidDeductionUtil.EffectDateBeforeMidMonth(employee.StartDate.ToDateTime(TimeOnly.MinValue)),
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };

    // Save to repository

    await _payrollRunService.AddRecordToCurrentRunAsync(deduction, employee.EmployeeId);
    await _medicalAidDeductionRepository.AddNewMedicalAidDeductionsAsync(deduction);

    return MapToDto(deduction);
  }

  public async Task<MedicalAidDeductionDto> UpdateDeductionByEmpId(string employeeId)
  {
    throw new NotImplementedException();
  }

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
    private static decimal CalculateAdultPremium(MedicalOptionDto? option, int numberOfAdults,CreateMedicalAidDeductionRequestDto request)
    {
      if (numberOfAdults <= 0) return 0m;

      decimal adultContribution = option.TotalMonthlyContributionsAdult;
      return adultContribution * numberOfAdults;
    }

    /// <summary>
    /// Calculates the child premium based on number of children and per-child contribution.
    /// </summary>
    private static decimal CalculateChildPremium(MedicalOptionDto? option, int numberOfChildren, CreateMedicalAidDeductionRequestDto request)
    {
      if (numberOfChildren <= 0) return 0m;

      decimal childContribution = option.TotalMonthlyContributionsChild;
      if (option.MedicalOptionName.Contains("Network") &&
          (option.MedicalOptionName[^1] > 0 && option.MedicalOptionName[^1] < 4))
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
}