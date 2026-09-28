using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Exceptions;

namespace LicenseGuard.Domain.ValueObjects
{
    public sealed class LicenseSignature : ValueObject
    {
        public string Value { get; }

        private LicenseSignature(string value)
        {
            Value = value;
        }

        public static LicenseSignature Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainValidationException("Signature cannot be empty.");
            var normalized = value.Trim();
            if (normalized.Length > 2048)
                throw new DomainValidationException("Signature cannot exceed 2048 characters.");

            return new LicenseSignature(normalized);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
