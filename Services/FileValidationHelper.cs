namespace FacultyInformationSystem_FIS_.Services
{
    public static class FileValidationHelper
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        // Returns null if valid, or an error message if not.
        public static string? Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return "Only PDF and Word documents (.pdf, .doc, .docx) are allowed.";
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return "File size must not exceed 10 MB.";
            }

            return null;
        }

        public static async Task<string> SaveAsync(IFormFile file, string subfolder, string webRootPath)
        {
            var uploadsFolder = Path.Combine(webRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{subfolder}/{uniqueFileName}";
        }
    }
}
