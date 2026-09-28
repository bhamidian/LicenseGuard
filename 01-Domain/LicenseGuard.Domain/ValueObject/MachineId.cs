using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Exceptions;

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
            if (value is null)
                throw new DomainValidationException("Machine ID is required.");

            var normalizedValue = value.Trim();

            if (normalizedValue.Length == 0)
                throw new DomainValidationException("Machine ID cannot be empty.");

            if (normalizedValue.Length > MaxLength)
                throw new DomainValidationException($"Machine ID cannot exceed {MaxLength} characters.");

            if (normalizedValue.Any(char.IsControl))
                throw new DomainValidationException("Machine ID cannot contain control characters.");

            return new MachineId(normalizedValue);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
