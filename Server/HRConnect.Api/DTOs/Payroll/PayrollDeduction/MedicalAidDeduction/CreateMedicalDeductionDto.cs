namespace HRConnect.Api.DTOs.Payroll.PayrollDeduction.MedicalAidDeduction
{

  public class CreateMedicalDeductionDto
  {

    //public int MedicalAidDeductionId { get; set; }
    //FK

    //public int PayrollRunId { get; set; }
    //FK

    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Branch { get; set; }

    public decimal Salary { get; set; }
    public DateTime EmployeeStartDate { get; set; }
    public DateTime EffectiveDate { get; set; } // This is the Medical Start Date
    //FK

    public int MedicalOptionId { get; set; }
    public string OptionName { get; set; }
    //FK

    public int MedicalCategoryId { get; set; }

    public string OptionCategory { get; set; }

    // Number of Deps
    public int PrincipalCount { get; set; }
    public int AdultCount { get; set; }
    public int ChildrenCount { get; set; }

    public decimal PrincipalPremium { get; set; }

    public decimal? SpousePremium { get; set; }

    public decimal? ChildPremium { get; set; }

    public decimal TotalDeductionAmount { get; set; }
    public DateTime CreatedDate { get; set; }// setting the default date to use UTC (Ask??)
    public bool IsActive { get; set; }
    public DateTime UpdatedDate { get; set; }
  }
}