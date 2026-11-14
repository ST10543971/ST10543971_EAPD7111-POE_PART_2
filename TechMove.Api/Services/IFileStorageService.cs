namespace TechMove.Api.Services;

public interface IFileStorageService
{
    Task<string> SavePdfAsync(Stream content, string extension, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);
}
