using System.Security.Cryptography;
using LicenseGuard.Domain.Exceptions;
using LicenseGuard.Domain.Entities;

namespace LicenseGuard.Domain.ValueObjects
{
    public sealed class LicenseKey : ValueObject
    {
        public string Value { get; }

        private LicenseKey(string value)
        {
            Value = value;
        }

        public static LicenseKey Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            var value = Convert.ToHexString(bytes);

            return new LicenseKey(value);
        }

        public static LicenseKey Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainValidationException("License key cannot be empty.");

            var normalized = value.Trim().ToUpperInvariant();
            if (normalized.Length != 64 || normalized.Any(character => !Uri.IsHexDigit(character)))
                throw new DomainValidationException("License key must be exactly 64 hexadecimal characters.");

            return new LicenseKey(normalized);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
