namespace HRConnect.Api.Services
{
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using HRConnect.Api.Interfaces;
    using Microsoft.AspNetCore.Http;
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string url, string publicId)> UploadFileAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            UploadResult result;
            if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "leave_documents"
                };

                result = await _cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "leave_documents"
                };

                result = await _cloudinary.UploadAsync(uploadParams);
            }
            if (result.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
            }

            return (result.SecureUrl.ToString(), result.PublicId);
        }
        public async Task DeleteFileAsync(string publicId)
        {
            var deleteParams = new CloudinaryDotNet.Actions.DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}