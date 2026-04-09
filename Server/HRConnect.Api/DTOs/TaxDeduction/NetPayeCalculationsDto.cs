using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.DTOs.TaxDeduction
{
  /// <summary>
  /// Input for the net PAYE estimate.
  /// The caller supplies everything the calculation needs so no employee
  /// record lookup is required – this works for both employees checking
  /// their own deduction and admins running a what-if scenario.
  /// </summary>
  public class NetPayeCalculationRequestDto
  {
    public decimal MonthlySalary { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public int SelectedPensionPercentage { get; set; }
    public bool HasMedicalAid { get; set; }
    public int AdultDependantCount { get; set; }
    public int ChildDependantCount { get; set; }
  }

  /// <summary>
  /// Full step-by-step breakdown of the monthly net PAYE calculation.
  /// Every intermediate figure is exposed so an employee or admin can see
  /// exactly how the final deduction was arrived at.
  /// </summary>
  public class NetPayeCalculationResultDto
  {
    public decimal MonthlySalary { get; set; }
    public decimal PensionPercentageApplied { get; set; }
    public decimal PensionContributionBeforeCap { get; set; }
    public bool PensionContributionWasCapped { get; set; }
    public decimal PensionContributionAfterCap { get; set; }
    public decimal TaxableIncome { get; set; }
    public int Age { get; set; }
    public string TaxBracketUsed { get; set; } = string.Empty;
    public decimal GrossTaxAmount { get; set; }
    public bool MedicalAidCreditApplied { get; set; }
    public MedicalTaxCreditBreakdownDto MedicalTaxCreditBreakdown { get; set; } = new();
    public decimal TotalMedicalTaxCredit { get; set; }
    public decimal NetPayeAmount { get; set; }
    public decimal EstimatedTakeHome { get; set; }
  }

  /// <summary>
  /// Itemised breakdown of each component of the medical tax credit
  /// so the employee can see exactly where each rand figure comes from.
  /// </summary>
  public class MedicalTaxCreditBreakdownDto
  {
    public decimal PrincipalMemberCredit { get; set; }
    public decimal AdultDependantCredit { get; set; }
    public decimal ChildDependantCredit { get; set; }
    public int AdultDependantCount { get; set; }
    public int ChildDependantCount { get; set; }
  }
}