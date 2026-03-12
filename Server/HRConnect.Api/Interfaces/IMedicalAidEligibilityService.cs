namespace HRConnect.Api.Interfaces
{
  using DTOs;

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
  }
}