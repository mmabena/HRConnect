namespace HRConnect.Api.DTOs.MedicalOption
{
  using System.Text.Json.Serialization;
  using HRConnect.Api.Models;

  public class MedicalOptionDto
  {
    // TODO: Use JsonPropertyName to override the property name in the JSON response
    [JsonPropertyName("optionId")]
    public int MedicalOptionId { get; set; }
    [JsonPropertyName("optionName")]
    public string MedicalOptionName { get; set; } = string.Empty;
    [JsonPropertyName("optionCategoryId")]
    public int MedicalOptionCategoryId { get; set; }
    public decimal? SalaryBracketMin { get; set; }
    public decimal? SalaryBracketMax { get; set; }
  }
}