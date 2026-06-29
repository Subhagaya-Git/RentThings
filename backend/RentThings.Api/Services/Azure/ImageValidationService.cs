using System.IO;
using System.Threading.Tasks;

namespace RentThings.Api.Services.Azure
{
    public class ImageValidationService : IImageValidationService
    {
        public async Task<(bool IsValid, string DetectedItems)> ValidateListingImageAsync(Stream imageStream)
        {
            // ටෙස්ට් කිරීමට දැනට true ලබා දෙන්න
            return (true, "Valid Image");
        }
    }
}