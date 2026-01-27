namespace VlogForge.Application.Common.Interfaces;

/// <summary>
/// Service interface for file storage operations.
/// Story: ACF-002
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the URL.
    /// </summary>
    /// <param name="stream">The file content stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="folder">Optional folder/prefix for organization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The URL of the uploaded file.</returns>
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its URL.
    /// </summary>
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a profile picture with multiple size variants.
    /// </summary>
    /// <param name="stream">The original image stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME type of the image.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The URL of the primary profile picture (400x400).</returns>
    Task<ProfilePictureUrls> UploadProfilePictureAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// URLs for profile picture variants.
/// Story: ACF-002
/// </summary>
public sealed record ProfilePictureUrls(
    string ThumbnailUrl,  // 150x150
    string StandardUrl    // 400x400
);
