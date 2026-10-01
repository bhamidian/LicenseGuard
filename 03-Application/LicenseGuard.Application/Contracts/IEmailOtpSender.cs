namespace LicenseGuard.Application.Contracts;

public interface IEmailOtpSender
{
    Task SendAsync(string emailAddress, string code, CancellationToken cancellationToken = default);
}
