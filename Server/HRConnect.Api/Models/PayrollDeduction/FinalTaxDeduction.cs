using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRConnect.Api.Models.Payroll;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRConnect.Api.Models.PayrollDeduction
{
  public class FinalTaxDeduction : PayrollRecord
  {
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }

    public int TaxYear { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlySalary { get; set; }

    public int MedicalAidMembers { get; set; }
    public int MedicalAidDependants { get; set; }
    public int MedicalAidChildren { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MedicalTaxCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionContribution { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionableIncome { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxDeductionAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UifEmployeeAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UifEmployerAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SdlAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }

    public string TaxCode { get; set; } = string.Empty;

  }
}