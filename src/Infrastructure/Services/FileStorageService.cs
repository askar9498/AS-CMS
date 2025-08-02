using AS_CMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AS_CMS.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _uploadPath;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _uploadPath = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder, string? customFileName = null)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            // Create folder path
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate filename
            var fileName = customFileName ?? $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.Information("File uploaded successfully: {FilePath}", filePath);

            return $"/uploads/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to upload file: {FileName}", file?.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var fullPath = Path.Combine(_uploadPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.Information("File deleted successfully: {FilePath}", filePath);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete file: {FilePath}", filePath);
            return false;
        }
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path is null or empty");
            }

            var fullPath = Path.Combine(_uploadPath, filePath.TrimStart('/'));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            return await File.ReadAllBytesAsync(fullPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get file: {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var fullPath = Path.Combine(_uploadPath, filePath.TrimStart('/'));
            return File.Exists(fullPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to check file existence: {FilePath}", filePath);
            return false;
        }
    }

    public string GetFileUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return string.Empty;
        }

        var baseUrl = _configuration["FileStorage:BaseUrl"] ?? "";
        return $"{baseUrl}{filePath}";
    }

    public async Task<string> UploadProfileImageAsync(IFormFile file, Guid userId)
    {
        return await UploadFileAsync(file, "profile-images", $"profile_{userId}_{Path.GetExtension(file.FileName)}");
    }

    public async Task<string> UploadResumeAsync(IFormFile file, Guid userId)
    {
        return await UploadFileAsync(file, "resumes", $"resume_{userId}_{Path.GetExtension(file.FileName)}");
    }

    public async Task<string> UploadLogoAsync(IFormFile file, Guid userId)
    {
        return await UploadFileAsync(file, "logos", $"logo_{userId}_{Path.GetExtension(file.FileName)}");
    }

    public async Task<string> UploadOfficialDocumentsAsync(IFormFile file, Guid userId)
    {
        return await UploadFileAsync(file, "official-documents", $"doc_{userId}_{Path.GetExtension(file.FileName)}");
    }
} 