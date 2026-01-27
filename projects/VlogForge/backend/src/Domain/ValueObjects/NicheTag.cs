using System.Text.RegularExpressions;

namespace VlogForge.Domain.ValueObjects;

/// <summary>
/// Value object representing a niche/category tag for a creator profile.
/// Story: ACF-002
/// </summary>
public sealed partial class NicheTag : IEquatable<NicheTag>
{
    public const int MinLength = 2;
    public const int MaxLength = 30;

    public string Value { get; }

    private NicheTag(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new NicheTag value object.
    /// </summary>
    /// <param name="value">The tag value (alphanumeric with hyphens, 2-30 chars).</param>
    /// <returns>A valid NicheTag instance.</returns>
    /// <exception cref="ArgumentException">Thrown when tag is invalid.</exception>
    public static NicheTag Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Niche tag cannot be empty.", nameof(value));
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length < MinLength)
        {
            throw new ArgumentException($"Niche tag must be at least {MinLength} characters.", nameof(value));
        }

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Niche tag cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!TagPattern().IsMatch(normalized))
        {
            throw new ArgumentException("Niche tag can only contain letters, numbers, and hyphens.", nameof(value));
        }

        return new NicheTag(normalized);
    }

    /// <summary>
    /// Tries to create a NicheTag without throwing exceptions.
    /// </summary>
    public static bool TryCreate(string value, out NicheTag? tag)
    {
        tag = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length < MinLength || normalized.Length > MaxLength)
            return false;

        if (!TagPattern().IsMatch(normalized))
            return false;

        tag = new NicheTag(normalized);
        return true;
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex TagPattern();

    public bool Equals(NicheTag? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as NicheTag);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(NicheTag? left, NicheTag? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(NicheTag? left, NicheTag? right) => !(left == right);

    public static implicit operator string(NicheTag tag) => tag.Value;
}
