namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;

/// <summary>
/// Request DTO for creating a new medical aid deduction.
/// Contains the selected medical option details from the eligibility check.
/// </summary>
public class CreateMedicalAidDeductionRequestDto
{
    /// <summary>
    /// The selected medical option ID from the eligible options.
    /// </summary>
    public int MedicalOptionId { get; set; }

    /// <summary>
    /// The medical category ID associated with the selected option.
    /// </summary>
    public int MedicalCategoryId { get; set; }

    /// <summary>
    /// Number of principal members (typically 1 - the employee themselves).
    /// </summary>
    public int PrincipalCount { get; set; } = 1;

    /// <summary>
    /// Number of adult dependents (spouses/partners).
    /// </summary>
    public int AdultCount { get; set; }

    /// <summary>
    /// Number of child dependents.
    /// </summary>
    public int ChildrenCount { get; set; }

    /// <summary>
    /// Calculated premium for the principal member.
    /// </summary>
    public decimal PrincipalPremium { get; set; }

    /// <summary>
    /// Calculated premium for adult dependents (if applicable).
    /// </summary>
    public decimal? SpousePremium { get; set; }

    /// <summary>
    /// Calculated premium for child dependents (if applicable).
    /// </summary>
    public decimal? ChildPremium { get; set; }

    /// <summary>
    /// Total monthly deduction amount (sum of all premiums).
    /// </summary>
    public decimal TotalDeductionAmount { get; set; }

    /// <summary>
    /// The effective date when the medical aid deduction should start.
    /// Defaults to current date if not specified.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }
}
