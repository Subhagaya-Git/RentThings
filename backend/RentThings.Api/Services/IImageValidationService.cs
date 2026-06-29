using System.IO;
using System.Threading.Tasks;

namespace RentThings.Api.Services.Azure
{
    public interface IImageValidationService
    {
        Task<(bool IsValid, string DetectedItems)> ValidateListingImageAsync(Stream imageStream);
    }
}