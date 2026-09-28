namespace LicenseGuard.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public sealed class DomainValidationException(string message) : DomainException(message);

public sealed class DomainRuleViolationException(string message) : DomainException(message);
