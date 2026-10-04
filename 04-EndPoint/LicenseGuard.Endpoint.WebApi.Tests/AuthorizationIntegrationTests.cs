using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Net.Http.Json;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Endpoint.WebApi.Controllers;
using LicenseGuard.Endpoint.WebApi.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace LicenseGuard.Endpoint.WebApi.Tests;

public sealed class AuthorizationIntegrationTests
{
    [Theory]
    [InlineData("POST", "/api/licenses", "{\"subscriptionId\":\"00000000-0000-0000-0000-000000000001\",\"maxActivations\":2}")]
    [InlineData("POST", "/api/subscriptions/00000000-0000-0000-0000-000000000001/renew", "{\"amount\":10,\"newExpirationDate\":\"2030-01-01T00:00:00Z\"}")]
    public async Task AdminRoutesRejectAnonymousRequests(string method, string path, string body)
    {
        using var host = await CreateTestHostAsync();
        using var client = host.GetTestClient();

        using var response = await client.SendAsync(JsonRequest(method, path, body));

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/licenses", "{\"subscriptionId\":\"00000000-0000-0000-0000-000000000001\",\"maxActivations\":2}")]
    [InlineData("POST", "/api/subscriptions/00000000-0000-0000-0000-000000000001/renew", "{\"amount\":10,\"newExpirationDate\":\"2030-01-01T00:00:00Z\"}")]
    public async Task AdminRoutesRejectCustomerRole(string method, string path, string body)
    {
        using var host = await CreateTestHostAsync();
        using var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Identity", "Customer");

        using var response = await client.SendAsync(JsonRequest(method, path, body));

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminCanReachProtectedRouteAndActorComesFromAuthenticatedPrincipal()
    {
        var adminId = Guid.NewGuid();
        var sender = new IntegrationSender();
        using var host = await CreateTestHostAsync(sender);
        using var client = host.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Identity", $"Admin:{adminId:D}");

        using var response = await client.PostAsync("/api/licenses", JsonContent.Create(new
        {
            subscriptionId = Guid.NewGuid(), maxActivations = 2
        }));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(adminId, Assert.IsType<LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand.CreateLicenseCommand>(sender.LastRequest).IssuedByAdminId);

        using var renewalResponse = await client.PostAsync($"/api/subscriptions/{Guid.NewGuid():D}/renew", JsonContent.Create(new
        {
            amount = 10m,
            newExpirationDate = DateTime.UtcNow.AddDays(30)
        }));

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, renewalResponse.StatusCode);
        Assert.Equal(adminId, Assert.IsType<LicenseGuard.Application.Features.Subscription.Commands.RenewSubscriptionCommand>(sender.LastRequest).RenewedByUserId);
    }

    [Theory]
    [InlineData("/api/licenses/activate", "{\"licenseKey\":\"key\",\"machineId\":\"machine\",\"instanceId\":\"instance\"}")]
    [InlineData("/api/licenses/validate", "{\"licenseKey\":\"key\",\"machineId\":\"machine\",\"instanceId\":\"instance\"}")]
    public async Task ClientLicenseRoutesRemainCallableWithoutAuthentication(string path, string body)
    {
        using var host = await CreateTestHostAsync();
        using var client = host.GetTestClient();

        using var response = await client.PostAsync(path, JsonContent.Create(body));

        Assert.NotEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerUiAndOpenApiDocumentExposeRoutesAndBearerAuth()
    {
        using var host = await CreateTestHostAsync();
        using var client = host.GetTestClient();

        using var uiResponse = await client.GetAsync("/swagger/index.html");
        using var documentResponse = await client.GetAsync("/swagger/v1/swagger.json");
        var document = System.Text.Json.JsonDocument.Parse(await documentResponse.Content.ReadAsStringAsync());

        Assert.Equal(System.Net.HttpStatusCode.OK, uiResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, documentResponse.StatusCode);
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/licenses", out var createPath));
        Assert.True(createPath.GetProperty("post").TryGetProperty("security", out _));
        Assert.True(paths.TryGetProperty("/api/licenses/activate", out var activatePath));
        Assert.False(activatePath.GetProperty("post").TryGetProperty("security", out _));
    }

    private static HttpRequestMessage JsonRequest(string method, string path, string body) =>
        new(new HttpMethod(method), path) { Content = JsonContent.Create(System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body)) };

    private static async Task<IHost> CreateTestHostAsync(IntegrationSender? sender = null)
    {
        var testSender = sender ?? new IntegrationSender();
        return await new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseEnvironment("Development")
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddLicenseGuardApi();
                    services.AddControllers().AddApplicationPart(typeof(LicenseController).Assembly);
                    services.AddAuthentication(TestAuthenticationHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });
                    services.AddAuthorization();
                    services.AddSingleton(testSender);
                    services.AddSingleton<ISender>(testSender);
                    services.AddSingleton<IAdminRepository>(new StubAdminRepository());
                })
                .Configure(app =>
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(options =>
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LicenseGuard API v1"));
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                }))
            .StartAsync();
    }

    private sealed class StubAdminRepository : IAdminRepository
    {
        public Task<Guid?> GetProfileIdByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(appUserId);
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Identity", out var identityValue))
                return Task.FromResult(AuthenticateResult.NoResult());

            var parts = identityValue.ToString().Split(':', 2);
            var claims = new List<Claim> { new(ClaimTypes.Role, parts[0]) };
            if (parts.Length == 2)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, parts[1]));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
        }
    }

    private sealed class IntegrationSender : ISender
    {
        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            var responseType = typeof(TResponse);
            var payloadType = responseType.GetGenericArguments()[0];
            var result = typeof(ResultDto<>).MakeGenericType(payloadType)
                .GetMethod(nameof(ResultDto<object>.Fail), [typeof(string), typeof(IEnumerable<string>), typeof(ResultFailureKind)])!
                .Invoke(null, ["Integration test dispatch", null, ResultFailureKind.Validation]);
            return Task.FromResult((TResponse)result!);
        }

        public Task<TResponse> Send<TResponse>(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => throw new NotSupportedException();
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
