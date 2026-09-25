using System.Security.Cryptography;
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
                throw new ArgumentException("License key cannot be empty.");

            if (value.Length != 64)
                throw new ArgumentException("Invalid license key.");

            return new LicenseKey(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}