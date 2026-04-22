namespace PharmacyApp.Infrastructure.Options;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    
    public string UploadPath { get; set; } = "wwwroot/uploads";
    public string ProductImagesPath { get; set; } = "product-images";
    public long MaxFileSize { get; set; } = 1024 * 1024 * 5;
    public string[] AllowedExtensions { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}