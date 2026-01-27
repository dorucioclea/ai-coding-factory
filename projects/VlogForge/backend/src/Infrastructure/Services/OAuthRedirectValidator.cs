using Microsoft.Extensions.Configuration;
using VlogForge.Application.Common.Interfaces;

namespace VlogForge.Infrastructure.Services;

/// <summary>
/// Validates OAuth redirect URIs against a configured whitelist.
/// Story: ACF-003
/// </summary>
public class OAuthRedirectValidator : IOAuthRedirectValidator
{
    private readonly HashSet<string> _allowedUris;
    private readonly List<string> _allowedUrisList;

    public OAuthRedirectValidator(IConfiguration configuration)
    {
        var uris = configuration.GetSection("OAuth:AllowedRedirectUris").Get<string[]>()
            ?? Array.Empty<string>();

        // Normalize URIs (remove trailing slashes, lowercase)
        _allowedUrisList = uris
            .Select(u => NormalizeUri(u))
            .Where(u => !string.IsNullOrEmpty(u))
            .Distinct()
            .ToList();

        _allowedUris = _allowedUrisList.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // In development, allow localhost if no URIs configured
        if (_allowedUris.Count == 0)
        {
            var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
            if (env.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                _allowedUris.Add("http://localhost:3000/auth/callback");
                _allowedUris.Add("http://localhost:5173/auth/callback");
                _allowedUris.Add("https://localhost:3000/auth/callback");
                _allowedUrisList.AddRange(_allowedUris);
            }
        }
    }

    public IReadOnlyList<string> AllowedUris => _allowedUrisList;

    public bool IsAllowed(string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            return false;
        }

        var normalized = NormalizeUri(redirectUri);
        return _allowedUris.Contains(normalized);
    }

    private static string NormalizeUri(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return string.Empty;
        }

        // Parse and normalize
        if (!Uri.TryCreate(uri.Trim(), UriKind.Absolute, out var parsed))
        {
            return string.Empty;
        }

        // Rebuild without trailing slash on path
        var path = parsed.AbsolutePath.TrimEnd('/');
        return $"{parsed.Scheme}://{parsed.Host}{(parsed.IsDefaultPort ? "" : $":{parsed.Port}")}{path}";
    }
}
