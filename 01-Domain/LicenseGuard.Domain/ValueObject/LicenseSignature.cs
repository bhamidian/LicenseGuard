using LicenseGuard.Domain.Entities;

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
                throw new ArgumentException("Signature cannot be empty.");

            return new LicenseSignature(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}