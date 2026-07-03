using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ClaimsModule.Domain.Interfaces;

namespace ClaimsModule.Infrastructure.Storage;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly string _containerName;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _containerName = configuration["Azure:ContainerName"] ?? "claim-documents";

        if (!string.IsNullOrEmpty(connectionString))
        {
            try
            {
                _blobServiceClient = new BlobServiceClient(connectionString);
                _logger.LogInformation("Successfully initialized Azure Blob Storage client with container: {Container}", _containerName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure Blob Storage Client. Fallback might be needed.");
            }
        }
        else
        {
            _logger.LogWarning("Azure Blob Storage connection string is missing. Azure Storage will run in disabled state.");
        }
    }

    public bool IsConfigured => _blobServiceClient != null;

    public async Task<string> UploadAsync(string containerPath, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        if (_blobServiceClient == null)
        {
            throw new InvalidOperationException("Azure Blob Storage service is not configured.");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        // Blob path layout: claim-documents/orgId/claimId/filename
        var blobName = $"{containerPath}/{fileName}".Replace("\\", "/");
        var blobClient = containerClient.GetBlobClient(blobName);

        var options = new Azure.Storage.Blobs.Models.BlobUploadOptions
        {
            HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, options, ct);
        _logger.LogInformation("Uploaded file {File} to Azure Blob container {Container}", blobName, _containerName);

        return blobName;
    }

    public async Task<string> GetDownloadUrlAsync(string blobPath, TimeSpan expiry, CancellationToken ct)
    {
        if (_blobServiceClient == null)
        {
            throw new InvalidOperationException("Azure Blob Storage service is not configured.");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (!await blobClient.ExistsAsync(ct))
        {
            throw new FileNotFoundException("Blob not found in Azure Storage.", blobPath);
        }

        // Generate Shared Access Signature (SAS) token for time-limited secure download
        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobPath,
                Resource = "b", // 'b' for blob
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Clock skew buffer
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        _logger.LogWarning("Cannot generate SAS Uri for blob {Blob}. Returning public Uri.", blobPath);
        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobPath, CancellationToken ct)
    {
        if (_blobServiceClient == null) return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        _logger.LogInformation("Deleted file {Blob} from Azure Blob container {Container}", blobPath, _containerName);
    }
}
