// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/FileService.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Upload files in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads [Accessed 31 May 2026].
//   - Microsoft. 2024. File and stream I/O. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/standard/io/ [Accessed 31 May 2026].
//   - Microsoft. 2024. Logging in C# and .NET. [Online]. Available at:
//     https://learn.microsoft.com/en-us/dotnet/core/extensions/logging [Accessed 31 May 2026].
// ==========================================================================

namespace GreenLogisticsMS.Api.Services
{
    /// <summary>
    /// Handles PDF uploads: validates extension/size, generates a UUID filename
    /// (prevents overwrites), saves to wwwroot/uploads, returns the relative path.
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;
        private const string UploadFolder = "uploads";

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<(string relativePath, string originalName)> SavePdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                throw new InvalidOperationException($"Invalid file type '{extension}'. Only .pdf files are allowed.");

            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds the 10 MB limit.");

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadPath = Path.Combine(webRoot, UploadFolder);
            Directory.CreateDirectory(uploadPath);

            var storedName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadPath, storedName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/{UploadFolder}/{storedName}";
            _logger.LogInformation("[FileService] Saved '{Original}' -> '{Path}'", file.FileName, relativePath);
            return (relativePath, file.FileName);
        }

        public string GetAbsolutePath(string relativePath)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var cleanPath = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(webRoot, cleanPath);
        }
    }
}
