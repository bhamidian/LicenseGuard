namespace LicenseGuard.Domain.Entities
{
    public class Admin : User
    {
        private Admin() { }

        public Admin(Guid appUserId) : base(appUserId) { }

        public ICollection<License> IssuedLicenses { get; private set; } = new List<License>();
    }
}
