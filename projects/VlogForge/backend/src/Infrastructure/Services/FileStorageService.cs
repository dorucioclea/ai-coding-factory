using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Settings for file storage service.
/// Story: ACF-002
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Gets or sets the storage provider type (Local, S3, MinIO).
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    /// Gets or sets the base path for local storage.
    /// </summary>
    public string LocalBasePath { get; set; } = "uploads";

    /// <summary>
    /// Gets or sets the base URL for accessing uploaded files.
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000/uploads";

    /// <summary>
    /// Gets or sets the S3 bucket name (for S3/MinIO).
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// Gets or sets the S3 endpoint URL (for MinIO).
    /// </summary>
    public string? EndpointUrl { get; set; }

    /// <summary>
    /// Gets or sets the S3 access key.
    /// </summary>
    public string? AccessKey { get; set; }

    /// <summary>
    /// Gets or sets the S3 secret key.
    /// </summary>
    public string? SecretKey { get; set; }
}

/// <summary>
/// Local file storage service implementation.
/// Story: ACF-002
/// </summary>
public sealed partial class FileStorageService : IFileStorageService
{
    private const int ThumbnailSize = 150;
    private const int StandardSize = 400;

    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<FileStorageSettings> settings, ILogger<FileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Ensure upload directory exists for local storage
        if (_settings.Provider == "Local")
        {
            Directory.CreateDirectory(_settings.LocalBasePath);
            Directory.CreateDirectory(Path.Combine(_settings.LocalBasePath, "profiles"));
            Directory.CreateDirectory(Path.Combine(_settings.LocalBasePath, "thumbnails"));
        }
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? folder = null,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileName = GenerateUniqueFileName(fileName);
        var relativePath = folder != null ? Path.Combine(folder, uniqueFileName) : uniqueFileName;

        if (_settings.Provider == "Local")
        {
            var fullPath = Path.Combine(_settings.LocalBasePath, relativePath);

            // Validate path to prevent directory traversal attacks
            var resolvedPath = Path.GetFullPath(fullPath);
            var basePath = Path.GetFullPath(_settings.LocalBasePath);
            if (!resolvedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                LogPathTraversalAttempt(_logger, relativePath);
                throw new InvalidOperationException("Invalid file path.");
            }

            var directory = Path.GetDirectoryName(resolvedPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = File.Create(resolvedPath);
            await stream.CopyToAsync(fileStream, cancellationToken);

            var url = $"{_settings.BaseUrl.TrimEnd('/')}/{relativePath.Replace('\\', '/')}";
            LogFileUploaded(_logger, url);
            return url;
        }

        // TODO: Implement S3/MinIO storage
        throw new NotImplementedException($"Storage provider '{_settings.Provider}' is not yet implemented.");
    }

    /// <inheritdoc />
    public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (_settings.Provider == "Local")
        {
            // Extract relative path from URL
            var relativePath = fileUrl.Replace(_settings.BaseUrl.TrimEnd('/'), "").TrimStart('/');
            var fullPath = Path.Combine(_settings.LocalBasePath, relativePath);

            // Validate path to prevent directory traversal attacks
            var resolvedPath = Path.GetFullPath(fullPath);
            var basePath = Path.GetFullPath(_settings.LocalBasePath);
            if (!resolvedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                LogPathTraversalAttempt(_logger, fileUrl);
                throw new InvalidOperationException("Invalid file path.");
            }

            if (File.Exists(resolvedPath))
            {
                File.Delete(resolvedPath);
                LogFileDeleted(_logger, resolvedPath);
            }

            return Task.CompletedTask;
        }

        // TODO: Implement S3/MinIO storage
        throw new NotImplementedException($"Storage provider '{_settings.Provider}' is not yet implemented.");
    }

    /// <inheritdoc />
    public async Task<ProfilePictureUrls> UploadProfilePictureAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            extension = contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".jpg"
            };
        }

        using var image = await Image.LoadAsync(stream, cancellationToken);

        // Create thumbnail (150x150)
        var thumbnailFileName = $"{uniqueId}_thumb{extension}";
        var thumbnailUrl = await SaveResizedImageAsync(image, ThumbnailSize, "thumbnails", thumbnailFileName, cancellationToken);

        // Create standard size (400x400)
        var standardFileName = $"{uniqueId}{extension}";
        var standardUrl = await SaveResizedImageAsync(image, StandardSize, "profiles", standardFileName, cancellationToken);

        LogProfilePictureUploaded(_logger, standardUrl, thumbnailUrl);

        return new ProfilePictureUrls(thumbnailUrl, standardUrl);
    }

    private async Task<string> SaveResizedImageAsync(
        Image image,
        int size,
        string folder,
        string fileName,
        CancellationToken cancellationToken)
    {
        // Clone and resize image
        using var resized = image.Clone(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(size, size),
                Mode = ResizeMode.Crop,
                Position = AnchorPositionMode.Center
            });
        });

        if (_settings.Provider == "Local")
        {
            var fullPath = Path.Combine(_settings.LocalBasePath, folder, fileName);
            await resized.SaveAsync(fullPath, cancellationToken);
            return $"{_settings.BaseUrl.TrimEnd('/')}/{folder}/{fileName}";
        }

        throw new NotImplementedException($"Storage provider '{_settings.Provider}' is not yet implemented.");
    }

    private static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var uniqueId = Guid.NewGuid().ToString("N");
        return $"{uniqueId}{extension}";
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "File uploaded: {Url}")]
    private static partial void LogFileUploaded(ILogger logger, string url);

    [LoggerMessage(Level = LogLevel.Information, Message = "File deleted: {Path}")]
    private static partial void LogFileDeleted(ILogger logger, string path);

    [LoggerMessage(Level = LogLevel.Information, Message = "Profile picture uploaded: {StandardUrl}, thumbnail: {ThumbnailUrl}")]
    private static partial void LogProfilePictureUploaded(ILogger logger, string standardUrl, string thumbnailUrl);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Path traversal attempt detected: {Path}")]
    private static partial void LogPathTraversalAttempt(ILogger logger, string path);
}
