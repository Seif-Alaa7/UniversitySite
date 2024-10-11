namespace HelwanUniversity.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadFile(IFormFile file, string currentUrl, string errorMessage);
    }
}
