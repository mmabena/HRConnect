namespace HRConnect.Api.Models
{
  public class TaxTableUpload
  {
    public int Id { get; set; }
    public int TaxYear { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
  }
}