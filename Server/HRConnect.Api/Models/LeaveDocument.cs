namespace HRConnect.Api.Models
{
    public class LeaveDocument
    {
        public int Id { get; set; }
        public int LeaveApplicationId { get; set; }
        public LeaveApplication LeaveApplication { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}