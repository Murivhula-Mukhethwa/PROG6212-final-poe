using System.Text.RegularExpressions;

namespace CMCS.web2.Services
{
    public static class FileValidationService
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        // File signatures (magic numbers) for validation
        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }, // %PDF
            { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }, // PK (zip archive)
            { ".xlsx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }  // PK (zip archive)
        };

        public static bool IsValidFile(IFormFile? file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null)
            {
                errorMessage = "No file was uploaded.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Invalid file type. Only PDF, DOCX, and XLSX are allowed.";
                return false;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                errorMessage = "File size exceeds 5 MB limit.";
                return false;
            }

            // Basic filename security check
            if (!IsValidFileName(file.FileName))
            {
                errorMessage = "Invalid file name.";
                return false;
            }

            return true;
        }

        public static async Task<bool> IsValidFileSignatureAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!FileSignatures.ContainsKey(extension))
                return false;

            using var stream = file.OpenReadStream();
            var signature = FileSignatures[extension];

            foreach (var expectedSignature in signature)
            {
                stream.Position = 0;
                var header = new byte[expectedSignature.Length];
                await stream.ReadAsync(header, 0, expectedSignature.Length);

                if (header.Take(expectedSignature.Length).SequenceEqual(expectedSignature))
                    return true;
            }

            return false;
        }

        private static bool IsValidFileName(string fileName)
        {
            // Check for path traversal attempts and other invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            if (fileName.Any(c => invalidChars.Contains(c)))
                return false;

            // Check for path traversal
            if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                return false;

            return true;
        }
    }
}