namespace HRConnect.Api.Models.SalaryBudget
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
    public class SalaryBudgetEmployee
    {
      /// <summary>
      /// Employee Information 
      /// </summary>
      ///
      public int Id { get; set; }
      public int SalaryBudgetId { get; set; }
      public SalaryBudget SalaryBudget {get; set; } = null!;
      public string EmployeeId { get; set; } = string.Empty;
      public Employee Employee { get; set; }  = null!;
       public string EmployeeName { get; set; } = string.Empty;
      public string Branch { get; set; } = string.Empty;
      public int JobGradeId { get; set; }
      public JobGrade? JobGrade { get; set; }  = null!;
      public string JobGradeName { get; set; } = string.Empty;
      public int PositionId { get; set;}
      public Position Position { get; set; }  = null!;
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
      public string Notes {get; set; } = string.Empty;
      public DateTime CreatedDate { get; set; } = DateTime.Now;
      public DateTime UpdatedDate { get; set; } = DateTime.Now;
      
    }
}