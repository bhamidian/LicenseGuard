namespace LicenseGuard.Application.Contracts;

public interface ISmsOtpSender
{
    Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
