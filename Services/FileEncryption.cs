using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CMCS.web2.Services
{
    public class FileEncryptionService : IFileEncryptionService
    {
        private readonly string _uploadPath;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public FileEncryptionService(IConfiguration config)
        {
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "EncryptedUploads");
            Directory.CreateDirectory(_uploadPath);

            // For demo: key + iv from config; in prod store in secure store
            var keyBase64 = config["FileEncryption:Key"];
            var ivBase64 = config["FileEncryption:IV"];
            if (string.IsNullOrEmpty(keyBase64) || string.IsNullOrEmpty(ivBase64))
            {
                using var aes = Aes.Create();
                _key = aes.Key;
                _iv = aes.IV;
            }
            else
            {
                _key = Convert.FromBase64String(keyBase64);
                _iv = Convert.FromBase64String(ivBase64);
            }
        }

        public async Task<string> EncryptAndSaveFileAsync(IFormFile file)
        {
            var storedFileName = $"{Guid.NewGuid()}.dat";
            var path = Path.Combine(_uploadPath, storedFileName);

            using var outFs = new FileStream(path, FileMode.Create);
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var cryptoStream = new CryptoStream(outFs, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await file.CopyToAsync(cryptoStream);
            await cryptoStream.FlushAsync();
            return storedFileName;
        }

        public async Task<byte[]> DecryptFileAsync(string storedFileName)
        {
            var path = Path.Combine(_uploadPath, storedFileName);
            if (!File.Exists(path)) throw new FileNotFoundException();

            using var inFs = new FileStream(path, FileMode.Open);
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var cryptoStream = new CryptoStream(inFs, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var ms = new MemoryStream();
            await cryptoStream.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
