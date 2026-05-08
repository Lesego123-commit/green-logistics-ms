// ==========================================================================
//  Green Logistics Management System (GLMS) - Part 3 SOA  |  PROG7311
//  File: GreenLogisticsMS.Api/Services/IFileService.cs
//  ----------------------------------------------------------------------
//  References (IIE Harvard):
//   - Microsoft. 2024. Upload files in ASP.NET Core. [Online]. Available at:
//     https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads [Accessed 31 May 2026].
// ==========================================================================

namespace GreenLogisticsMS.Api.Services
{
    public interface IFileService
    {
        Task<(string relativePath, string originalName)> SavePdfAsync(IFormFile file);
        string GetAbsolutePath(string relativePath);
    }
}
