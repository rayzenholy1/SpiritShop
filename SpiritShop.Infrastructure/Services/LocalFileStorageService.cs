using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SpiritShop.Application.Interfaces;

namespace SpiritShop.Infrastructure.Services;


public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _uploadsFolder = configuration["FileStorage:UploadsFolder"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "https://localhost:5001";

        Directory.CreateDirectory(_uploadsFolder);
    }

    public async Task<string> SaveFileAsync(
        IFormFile file,
        string fileKey,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{fileKey}{extension}";
        var filePath = Path.Combine(_uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream, cancellationToken);

        return fileName;
    }

    public string GetFileUrl(string fileKey)
    {
        var files = Directory.GetFiles(_uploadsFolder, $"{fileKey}.*");
        if (files.Length == 0) return string.Empty;

        var fileName = Path.GetFileName(files[0]);
        return $"{_baseUrl}/uploads/{fileName}";
    }

    public async Task DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(_uploadsFolder, $"{fileKey}.*");
        foreach (var file in files)
        {
            File.Delete(file);
        }
        await Task.CompletedTask;
    }
}
