using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRConnect.Api.DTOs.TaxDeduction
{
  public class TaxCalculationDto
  {
    public int MedicalAidMembers { get; set; }
    public int MedicalAidDependants { get; set; }
    public int MedicalAidChildren { get; set; }

  }
}