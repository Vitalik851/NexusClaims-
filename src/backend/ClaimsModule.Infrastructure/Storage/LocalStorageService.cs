using ClaimsModule.Domain.Interfaces;

namespace ClaimsModule.Infrastructure.Storage;

public class LocalStorageService : IStorageService
{
    private readonly string _storageRoot;

    public LocalStorageService()
    {
        // Save files in the API project's wwwroot/uploads directory so they can be downloaded
        _storageRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<string> UploadAsync(string containerPath, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        var targetFolder = Path.Combine(_storageRoot, containerPath);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        var filePath = Path.Combine(targetFolder, fileName);
        
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        // Return relative path to be saved in DB
        return Path.Combine("uploads", containerPath, fileName).Replace("\\", "/");
    }

    public Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan expiry, CancellationToken ct)
    {
        // For local development, download URL points to the local HTTP endpoint (e.g. http://localhost:5000/uploads/...)
        // The host part will be resolved by the API layer, so we return the relative path
        var relativeUrl = $"/{blobPath.Replace("\\", "/")}";
        return Task.FromResult(relativeUrl);
    }

    public Task DeleteAsync(string blobPath, CancellationToken ct)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", blobPath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}
