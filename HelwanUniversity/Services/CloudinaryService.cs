// Services/CloudinaryService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace HelwanUniversity.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string> UploadFile(IFormFile file, string currentUrl, string errorMessage)
        {
            if (file != null && file.Length > 0)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return uploadResult.SecureUrl.AbsoluteUri;
                    }
                    else
                    {
                        throw new Exception(errorMessage);
                    }
                }
            }
            return currentUrl;
        }
    }
}