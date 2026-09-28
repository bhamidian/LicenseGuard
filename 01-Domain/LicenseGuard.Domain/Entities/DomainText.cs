using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.Entities;

internal static class DomainText
{
    public static string Required(string value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{parameterName} is required.");
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new DomainValidationException($"{parameterName} cannot exceed {maxLength} characters.");
        return normalized;
    }

    public static string Optional(string? value, int maxLength, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length > maxLength)
            throw new DomainValidationException($"{parameterName} cannot exceed {maxLength} characters.");
        return normalized;
    }
}
