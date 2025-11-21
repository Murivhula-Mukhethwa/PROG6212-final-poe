using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CMCS.web2.Services
{
    public interface IFileEncryptionService
    {
        Task<string> EncryptAndSaveFileAsync(IFormFile file);
        Task<byte[]> DecryptFileAsync(string storedFileName);
    }
}
