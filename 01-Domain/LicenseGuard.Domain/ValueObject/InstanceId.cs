using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.ValueObjects
{
    public sealed class InstanceId : ValueObject
    {
        private const int MaxLength = 256;

        public string Value { get; }

        private InstanceId(string value)
        {
            Value = value;
        }

        public static InstanceId Create(string value)
        {
            if (value is null)
                throw new DomainValidationException("Instance ID is required.");

            var normalizedValue = value.Trim();

            if (normalizedValue.Length == 0)
                throw new DomainValidationException("Instance ID cannot be empty.");

            if (normalizedValue.Length > MaxLength)
                throw new DomainValidationException($"Instance ID cannot exceed {MaxLength} characters.");

            if (normalizedValue.Any(char.IsControl))
                throw new DomainValidationException("Instance ID cannot contain control characters.");

            return new InstanceId(normalizedValue);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
