namespace HRConnect.Api.Interfaces
{
    using Microsoft.AspNetCore.Http;
    public interface ICloudinaryService
    {
        Task<(string url, string publicId)> UploadFileAsync(IFormFile file);
        Task DeleteFileAsync(string publicId);
    }
}