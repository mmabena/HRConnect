namespace HRConnect.Api.DTOs
{
  public class CreatePensionFundDto
  {
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TaxCode { get; set; }
  }
}
