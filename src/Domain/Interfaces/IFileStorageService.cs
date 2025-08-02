using Microsoft.AspNetCore.Http;

namespace AS_CMS.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folder, string? customFileName = null);
    Task<bool> DeleteFileAsync(string filePath);
    Task<byte[]> GetFileAsync(string filePath);
    Task<bool> FileExistsAsync(string filePath);
    string GetFileUrl(string filePath);
    Task<string> UploadProfileImageAsync(IFormFile file, Guid userId);
    Task<string> UploadResumeAsync(IFormFile file, Guid userId);
    Task<string> UploadLogoAsync(IFormFile file, Guid userId);
    Task<string> UploadOfficialDocumentsAsync(IFormFile file, Guid userId);
} 