using LicenseGuard.Domain.Entities;

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
            ArgumentNullException.ThrowIfNull(value);

            var normalizedValue = value.Trim();

            if (normalizedValue.Length == 0)
                throw new ArgumentException("Instance ID cannot be empty.", nameof(value));

            if (normalizedValue.Length > MaxLength)
                throw new ArgumentException($"Instance ID cannot exceed {MaxLength} characters.", nameof(value));

            if (normalizedValue.Any(char.IsControl))
                throw new ArgumentException("Instance ID cannot contain control characters.", nameof(value));

            return new InstanceId(normalizedValue);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
