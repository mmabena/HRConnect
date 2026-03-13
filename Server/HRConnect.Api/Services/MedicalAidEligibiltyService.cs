namespace HRConnect.Api.Services
{
  using DTOs;
  using DTOs.Employee;
  using DTOs.MedicalOption;
  using Interfaces;
  using Models;

  /// <summary>
  /// Service implementation for determining employee medical aid eligibility.
  /// Calculates eligible medical options based on employee salary and dependents.
  /// </summary>
  public class MedicalAidEligibilityService : IMedicalAidEligibilityService
  {
    private readonly IEmployeeService _employeeService;
    private readonly IMedicalOptionRepository _medicalOptionRepository;

    /// <summary>
    /// Initializes a new instance of the MedicalAidEligibilityService class.
    /// </summary>
    /// <param name="employeeService">Service for retrieving employee details.</param>
    /// <param name="medicalOptionRepository">Repository for accessing medical options.</param>
    public MedicalAidEligibilityService(
        IEmployeeService employeeService,
        IMedicalOptionRepository medicalOptionRepository)
    {
      _employeeService = employeeService;
      _medicalOptionRepository = medicalOptionRepository;
    }

    /// <summary>
    /// Gets eligible medical options for an employee based on their salary and dependents.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="request">Request containing dependent information.</param>
    /// <returns>List of eligible medical options with calculated premiums.</returns>
    /// <exception cref="ArgumentException">Thrown when employeeId is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when employee is not found.</exception>
    public async Task<IReadOnlyList<ResponseEligibileOptionsDto>> GetEligibleMedicalOptionsForEmployeeAsync(
        string employeeId,
        RequestEligibileOptionsDto request)
    {
      if (string.IsNullOrWhiteSpace(employeeId))
      {
        throw new ArgumentException("Employee ID cannot be null or empty", nameof(employeeId));
      }

      // Get employee details
      var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
      if (employee == null)
      {
        throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
      }

      // Calculate total dependents
      int totalDependents = request.NumberOfPrincipals + request.NumberOfAdults + request.NumberOfChildren;

      // Get all medical options grouped by category
      var groupedOptions = await _medicalOptionRepository.GetGroupedMedicalOptionsAsync();

      var eligibleOptions = new List<ResponseEligibileOptionsDto>();

      foreach (var group in groupedOptions)
      {
        foreach (var option in group)
        {
          // Check if employee qualifies based on salary bracket
          if (IsEmployeeEligibleForOption(employee, option))
          {
            var responseDto = MapToResponseDto(employee, option, request, totalDependents);
            eligibleOptions.Add(responseDto);
          }
        }
      }

      return eligibleOptions.AsReadOnly();
    }

    /// <summary>
    /// Determines if an employee is eligible for a specific medical option based on salary bracket.
    /// </summary>
    /// <param name="employee">The employee to check.</param>
    /// <param name="option">The medical option to evaluate.</param>
    /// <returns>True if the employee qualifies for the option; otherwise, false.</returns>
    private static bool IsEmployeeEligibleForOption(EmployeeDto employee, MedicalOption option)
    {
      // Check salary bracket eligibility
      bool withinMinBracket = !option.SalaryBracketMin.HasValue ||
                              employee.MonthlySalary >= option.SalaryBracketMin.Value;

      bool withinMaxBracket = !option.SalaryBracketMax.HasValue ||
                              employee.MonthlySalary <= option.SalaryBracketMax.Value;

      return withinMinBracket && withinMaxBracket;
    }

    /// <summary>
    /// Maps medical option and employee data to a response DTO with calculated premiums.
    /// </summary>
    /// <param name="employee">The employee details.</param>
    /// <param name="option">The medical option.</param>
    /// <param name="request">The request containing dependent counts.</param>
    /// <param name="totalDependents">Total number of dependents.</param>
    /// <returns>A populated ResponseEligibileOptionsDto.</returns>
    private static ResponseEligibileOptionsDto MapToResponseDto(
        EmployeeDto employee,
        MedicalOption option,
        RequestEligibileOptionsDto request,
        int totalDependents)
    {
      // Calculate estimated premiums
      decimal principalPremium = CalculatePrincipalPremium(option);
      decimal adultPremium = CalculateAdultPremium(option, request.NumberOfAdults);
      decimal childPremium = CalculateChildPremium(option, request.NumberOfChildren);
      decimal totalPremium = principalPremium + adultPremium + childPremium;

      return new ResponseEligibileOptionsDto
      {
        // Employee details
        EmployeeName = employee.Name,
        EmployeeSurname = employee.Surname,
        Salary = employee.MonthlySalary,
        NumberOfPrincipals = request.NumberOfPrincipals,
        NumberOfAdults = request.NumberOfAdults,
        NumberOfChildren = request.NumberOfChildren,

        // Medical option details
        MedicalOptionId = option.MedicalOptionId,
        MedicalOptionName = option.MedicalOptionName,
        MedicalOptionCategoryName = option.MedicalOptionCategory?.MedicalOptionCategoryName ?? string.Empty,
        MedicalOptionCategoryId = option.MedicalOptionCategoryId,
        SalaryBracketMin = option.SalaryBracketMin,
        SalaryBracketMax = option.SalaryBracketMax,

        // Contribution details
        MonthlyRiskContributionPrincipal = option.MonthlyRiskContributionPrincipal,
        MonthlyRiskContributionAdult = option.MonthlyRiskContributionAdult,
        MonthlyRiskContributionChild = option.MonthlyRiskContributionChild,
        MonthlyRiskContributionChild2 = option.MonthlyRiskContributionChild2,
        MonthlyMsaContributionPrincipal = option.MonthlyMsaContributionPrincipal,
        MonthlyMsaContributionAdult = option.MonthlyMsaContributionAdult,
        MonthlyMsaContributionChild = option.MonthlyMsaContributionChild,
        TotalMonthlyContributionsPrincipal = option.TotalMonthlyContributionsPrincipal,
        TotalMonthlyContributionsAdult = option.TotalMonthlyContributionsAdult,
        TotalMonthlyContributionsChild = option.TotalMonthlyContributionsChild,
        TotalMonthlyContributionsChild2 = option.TotalMonthlyContributionsChild2,

        // Premium breakdown
        EstimatedPrincipalMonthlyPremium = principalPremium,
        EstimatedAdultMonthlyPremium = adultPremium > 0 ? adultPremium : null,
        EstimatedChildMonthlyPremium = childPremium > 0 ? childPremium : null,
        EstimatedTotalMonthlyPremium = totalPremium
      };
    }

    /// <summary>
    /// Calculates the principal premium based on total monthly contributions.
    /// </summary>
    private static decimal CalculatePrincipalPremium(MedicalOption option)
    {
      return option.TotalMonthlyContributionsPrincipal ?? 0m;
    }

    /// <summary>
    /// Calculates the adult premium based on number of adults and per-adult contribution.
    /// </summary>
    private static decimal CalculateAdultPremium(MedicalOption option, int numberOfAdults)
    {
      if (numberOfAdults <= 0) return 0m;

      decimal adultContribution = option.TotalMonthlyContributionsAdult;
      return adultContribution * numberOfAdults;
    }

    /// <summary>
    /// Calculates the child premium based on number of children and per-child contribution.
    /// </summary>
    private static decimal CalculateChildPremium(MedicalOption option, int numberOfChildren)
    {
      if (numberOfChildren <= 0) return 0m;

      decimal childContribution = option.TotalMonthlyContributionsChild;
      return childContribution * numberOfChildren;
    }
  }
}