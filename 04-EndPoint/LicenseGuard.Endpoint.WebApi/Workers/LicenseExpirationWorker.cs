using LicenseGuard.Application.Features.License.Commands.ExpireLicenses;
using MediatR;

namespace LicenseGuard.Endpoint.WebApi.Workers;

public sealed class LicenseExpirationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<LicenseExpirationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await sender.Send(new ExpireLicensesCommand(), stoppingToken);
                if (!result.IsSuccess)
                    logger.LogError("License expiration job failed validation: {Errors}",
                        string.Join("; ", result.Errors));
                else if (result.Data > 0)
                    logger.LogInformation("Automatically expired {Count} licenses.", result.Data);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "License expiration job failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
