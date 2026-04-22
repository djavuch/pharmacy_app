using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PharmacyApp.Application.Interfaces.Abstractions;
using PharmacyApp.Infrastructure.Options;

namespace PharmacyApp.Infrastructure.Services.FileStorage;

public sealed class LocalImageStorageService : IImageStorageService
{
    private readonly FileStorageOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public LocalImageStorageService(
        IOptions<FileStorageOptions> options,
        IHttpContextAccessor httpContextFactory,
        IWebHostEnvironment webHostEnvironment)
    {
        _options = options.Value;
        _httpContextAccessor = httpContextFactory;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName,
        string contentType, CancellationToken ct = default)
    {
        if (!_options.AllowedExtensions.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidDataException("Unsupported file type.");

        var extension = Path.GetExtension(fileName);
        var generatedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativeFolder = Path.Combine(_options.UploadPath, _options.ProductImagesPath);
        var absoluteFolder = Path.IsPathRooted(relativeFolder)
            ? relativeFolder
            : Path.Combine(_webHostEnvironment.ContentRootPath, relativeFolder);
        
        Directory.CreateDirectory(absoluteFolder);
        
        var absolutePath = Path.Combine(absoluteFolder, generatedFileName);
        
        await using var output = File.Create(absolutePath);
        await fileStream.CopyToAsync(output, ct);
        
        var request = _httpContextAccessor.HttpContext?.Request 
                      ?? throw new InvalidOperationException("Http context is unavailable.");

        var uploadPath = _options.UploadPath
            .Replace('\\', '/')
            .Trim('/');

        if (uploadPath.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
        {
            uploadPath = uploadPath["wwwroot/".Length..];
        }

        var urlPath = string.Join('/',
            new[] { uploadPath, _options.ProductImagesPath, generatedFileName }
                .Where(pathPart => !string.IsNullOrWhiteSpace(pathPart))
                .Select(pathPart => pathPart.Trim('/')));
        
        return $"{request.Scheme}://{request.Host}/{urlPath}";
    }
}
