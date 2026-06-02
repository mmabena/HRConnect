namespace HRConnect.Api.DTOs.SalaryBudgetEmployee
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;

  public class CreateBudgetEmployeeDto
  {
    /// <summary>
    /// Employee Information 
    /// </summary>
    ///
    public int SalaryBudgetId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public int JobGradeId { get; set; }
    public string JobGradeName { get; set; } = string.Empty;
    public int PositionId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    /// <summary>
    /// Salaries & Increases
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentSalary { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ProposedPercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NewAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CarAllowance { get; set; }
    /// <summary>
    /// Benefits
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? BonusApril { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BonusOctober { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DeathBenefit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DisabilityBenefit { get; set; }
    /// <summary>
    /// Status & Timeline
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// Cost to Company
    /// </summary>

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCostToCompany { get; set; }
    /// <summary>
    /// Notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime UpdatedDate { get; set; } = DateTime.Now;

  }
}