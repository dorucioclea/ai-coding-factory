namespace VlogForge.Domain.ValueObjects;

/// <summary>
/// Value object representing a creator's bio/description.
/// Story: ACF-002
/// </summary>
public sealed class Bio : IEquatable<Bio>
{
    public const int MaxLength = 500;

    public string Value { get; }

    private Bio(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Bio value object.
    /// </summary>
    /// <param name="value">The bio text (can be empty, max 500 chars).</param>
    /// <returns>A valid Bio instance.</returns>
    /// <exception cref="ArgumentException">Thrown when bio exceeds max length.</exception>
    public static Bio Create(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Bio cannot exceed {MaxLength} characters.", nameof(value));
        }

        return new Bio(trimmed);
    }

    /// <summary>
    /// Creates an empty Bio.
    /// </summary>
    public static Bio Empty => new(string.Empty);

    public bool Equals(Bio? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as Bio);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(Bio? left, Bio? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Bio? left, Bio? right) => !(left == right);

    public static implicit operator string(Bio bio) => bio.Value;
}
