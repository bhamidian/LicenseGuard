using LicenseGuard.Domain.Entities;

namespace LicenseGuard.Domain.ValueObjects
{
    public sealed class MachineId : ValueObject
    {
        private const int MaxLength = 256;

        public string Value { get; }

        private MachineId(string value)
        {
            Value = value;
        }

        public static MachineId Create(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var normalizedValue = value.Trim();

            if (normalizedValue.Length == 0)
                throw new ArgumentException("Machine ID cannot be empty.", nameof(value));

            if (normalizedValue.Length > MaxLength)
                throw new ArgumentException($"Machine ID cannot exceed {MaxLength} characters.", nameof(value));

            if (normalizedValue.Any(char.IsControl))
                throw new ArgumentException("Machine ID cannot contain control characters.", nameof(value));

            return new MachineId(normalizedValue);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
