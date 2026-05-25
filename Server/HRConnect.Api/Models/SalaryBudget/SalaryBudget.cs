namespace HRConnect.Api.Models.SalaryBudget
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Linq;
  using System.Threading.Tasks;
    public class SalaryBudget
    {
      /// <summary>
      /// Employee Information 
      /// </summary>
      /// 
      public int SalaryBudgetId { get; set; }
      public string EmployeeId { get; set; }
      public string EmployeeName { get; set; }
      public Employee Employee { get; set; }
      public string Branch { get; set; }
      public int JobGradeId { get; set; }
      public JobGrade? JobGrade { get; set; }
      public string JobTitle { get; set; }
      public Position Position { get; set; }

      /// <summary>
      /// Salaries & Increases
      /// </summary>
      [Column(TypeName = "decimal(18,2)")]
      public decimal CurrentSalary { get; set; }
      public decimal? ProposedPercentage { get; set; }  
      public decimal NewAmount { get; set; }
      public decimal CarAllowance { get; set; }
      /// <summary>
      /// Benefits
      /// </summary>
      public decimal BonusApril { get; set; }
      public decimal BonusOctober { get; set; }
      public decimal DeathBenefit { get; set; }
      public decimal DisabilityBenefit { get; set; }
      /// <summary>
      /// Status & Timeline
      /// </summary>
      public string Status { get; set; }
      public DateTime StartDate { get; set; }
      public DateTime? EndDate { get; set; }
      /// <summary>
      /// Cost to Company
      /// </summary>
      public decimal GrossSalary { get; set; }
      public decimal TotalCostToCompany { get; set; }
      /// <summary>
      /// Notes
      /// </summary>
      public string Notes {get; set;}
      public DateTime CreatedDate { get; set; } = DateTime.Now;
      public DateTime UpdatedDate { get; set; } = DateTime.Now;
      
    }
}