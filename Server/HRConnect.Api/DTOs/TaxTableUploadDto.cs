namespace HRConnect.Api.DTOs
{
  public class TaxTableUploadDto
  {
    public int Id { get; set; }
    public int TaxYear { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
  }
}