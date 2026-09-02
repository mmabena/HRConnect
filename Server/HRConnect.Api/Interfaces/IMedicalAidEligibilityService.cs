namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.DTOs;
  using System;
  using HRConnect.Api.Models;

  /// <summary>
  /// Defines the contract for medical aid eligibility operations.
  /// Provides methods for determining which medical options an employee qualifies for.
  /// </summary>
  public interface IMedicalAidEligibilityService
  {
    /// <summary>
    /// Gets eligible medical options for an employee based on their salary and dependents.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee.</param>
    /// <param name="request">Request DTO containing dependent information (principals, adults, children).</param>
    /// <returns>List of eligible medical options with calculated premiums.</returns>
    Task<IReadOnlyList<ResponseEligibileOptionsDto>> GetEligibleMedicalOptionsForEmployeeAsync(
        string employeeId,
        RequestEligibileOptionsDto request);

    Task<bool> isEligibleAsync(string employeeId,
      int medicalOptionId, int principalCount, int adultCount, int childCount);

    Task<IReadOnlyList<ResponseEligibileOptionsDto>> GetEligibleMedicalOptionsForOnboardingAsync(
   decimal salary,
   EmploymentStatus employmentStatus,
   string employeeName,
   string employeeSurname,
   RequestEligibileOptionsDto request);
  }
}