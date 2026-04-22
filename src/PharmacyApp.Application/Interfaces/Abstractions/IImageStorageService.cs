namespace PharmacyApp.Application.Interfaces.Abstractions;

public interface IImageStorageService
{
   Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken ct = default); 
}