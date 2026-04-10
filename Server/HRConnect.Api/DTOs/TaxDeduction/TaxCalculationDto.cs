using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.DTOs.TaxDeduction
{
  public class TaxCalculationDto
  {
    public int EmployeeId { get; set; }
    public int PayRunId { get; set; }

    // ONLY USER INPUTS
    public int MedicalAidMembers { get; set; }
    public int MedicalAidDependants { get; set; }
    public int MedicalAidChildren { get; set; }

    // Explanation field 
    public string Explanation { get; set; } = string.Empty;
  }
}