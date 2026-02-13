namespace HRConnect.Api.DTOs.MedicalOption
{
  public class MedicalOptionSalaryDto
  {
    public int MedicalOptionID { get; set; }
    public string MedicalOptionName { get; set; }
    public int MedicalOptionCategoryId { get; set; }
    public decimal SalaryBracketMin { get; set; }
    public decimal SalaryBracketMax { get; set; }
  }
}