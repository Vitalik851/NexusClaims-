namespace ClaimsModule.Domain.Interfaces;

/// <summary>
/// Abstraction over blob / file storage for claim documents.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file to the specified container path and returns the blob path.
    /// </summary>
    /// <param name="containerPath">Logical container or directory path.</param>
    /// <param name="fileName">Name of the file being uploaded.</param>
    /// <param name="content">File content stream.</param>
    /// <param name="contentType">MIME content type.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The blob path where the file was stored.</returns>
    Task<string> UploadAsync(
        string containerPath,
        string fileName,
        Stream content,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Generates a time-limited download URL for the specified blob.
    /// </summary>
    /// <param name="blobPath">The blob path returned from a previous upload.</param>
    /// <param name="expiry">How long the URL should remain valid.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A pre-signed download URL.</returns>
    Task<string> GetDownloadUrlAsync(
        string blobPath,
        TimeSpan expiry,
        CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes a blob from storage.
    /// </summary>
    /// <param name="blobPath">The blob path to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeleteAsync(
        string blobPath,
        CancellationToken ct = default);
}
