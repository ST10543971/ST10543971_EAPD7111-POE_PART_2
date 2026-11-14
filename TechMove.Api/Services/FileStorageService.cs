namespace TechMove.Api.Services;

public sealed class FileStorageService(IWebHostEnvironment environment, IConfiguration configuration) : IFileStorageService
{
    public async Task<string> SavePdfAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        var rootPath = configuration["FileStorage:RootPath"];
        var directory = string.IsNullOrWhiteSpace(rootPath)
            ? Path.Combine(environment.ContentRootPath, "Uploads")
            : rootPath;
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(directory, fileName);
        await using var target = File.Create(fullPath);
        await content.CopyToAsync(target, cancellationToken);
        return fileName;
    }

    public Task<Stream?> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var rootPath = configuration["FileStorage:RootPath"];
        var directory = string.IsNullOrWhiteSpace(rootPath)
            ? Path.Combine(environment.ContentRootPath, "Uploads")
            : rootPath;
        var safeName = Path.GetFileName(relativePath);
        var path = Path.Combine(directory, safeName);
        Stream? stream = File.Exists(path) ? File.OpenRead(path) : null;
        return Task.FromResult(stream);
    }
}
