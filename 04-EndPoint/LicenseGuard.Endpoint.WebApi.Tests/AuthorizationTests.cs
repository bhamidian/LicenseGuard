using System.Reflection;
using System.Security.Claims;
using LicenseGuard.Application.Features.License.Commands.CreateLicensesCommand;
using LicenseGuard.Application.Features.Subscription.Commands;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Endpoint.WebApi.Controllers;
using LicenseGuard.Endpoint.WebApi.Records;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LicenseGuard.Endpoint.WebApi.Tests;

public sealed class AuthorizationTests
{
    [Fact]
    public void LicenseCreationRequiresAdminRole()
    {
        var authorization = GetAuthorization<LicenseController>(nameof(LicenseController.Create));

        Assert.Equal("Admin", authorization.Roles);
    }

    [Fact]
    public void SubscriptionRenewalRequiresAdminRole()
    {
        var authorization = GetAuthorization<SubscriptionController>(nameof(SubscriptionController.Renew));

        Assert.Equal("Admin", authorization.Roles);
    }

    [Theory]
    [InlineData(nameof(LicenseController.Activate))]
    [InlineData(nameof(LicenseController.Validate))]
    public void LicenseClientEndpointsRemainAnonymous(string actionName)
    {
        var method = typeof(LicenseController).GetMethod(actionName)!;

        Assert.Null(method.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public async Task CreateLicenseUsesAuthenticatedAdminId()
    {
        var adminId = Guid.NewGuid();
        var sender = new CapturingSender();
        var controller = WithUser(new LicenseController(sender, new StubAdminRepository(adminId)), adminId);

        await controller.Create(new CreateLicenseRequest(Guid.NewGuid(), 3), CancellationToken.None);

        var command = Assert.IsType<CreateLicenseCommand>(sender.LastRequest);
        Assert.Equal(adminId, command.IssuedByAdminId);
        Assert.Equal(adminId, command.IssuedByUserId);
    }

    [Fact]
    public async Task RenewSubscriptionUsesAuthenticatedAdminId()
    {
        var adminId = Guid.NewGuid();
        var sender = new CapturingSender();
        var controller = WithUser(new SubscriptionController(sender), adminId);

        await controller.Renew(Guid.NewGuid(), new RenewSubscriptionRequest(10m, DateTime.UtcNow.AddDays(30)),
            CancellationToken.None);

        var command = Assert.IsType<RenewSubscriptionCommand>(sender.LastRequest);
        Assert.Equal(adminId, command.RenewedByUserId);
    }

    [Fact]
    public async Task MissingUserIdClaimForbidsLicenseCreationWithoutDispatch()
    {
        var sender = new CapturingSender();
        var controller = new LicenseController(sender, new StubAdminRepository(null))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.Create(new CreateLicenseRequest(Guid.NewGuid(), 1), CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        Assert.Null(sender.LastRequest);
    }

    private sealed class StubAdminRepository(Guid? profileId) : IAdminRepository
    {
        public Task<Guid?> GetProfileIdByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default) =>
            Task.FromResult(profileId);
    }

    private static AuthorizeAttribute GetAuthorization<TController>(string actionName)
    {
        var method = typeof(TController).GetMethod(actionName)!;
        return Assert.IsType<AuthorizeAttribute>(method.GetCustomAttribute<AuthorizeAttribute>());
    }

    private static TController WithUser<TController>(TController controller, Guid userId)
        where TController : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, userId.ToString("D"))], "test"))
            }
        };
        return controller;
    }

    private sealed class CapturingSender : ISender
    {
        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            var resultType = typeof(TResponse);
            var payloadType = resultType.GetGenericArguments()[0];
            var result = typeof(ResultDto<>).MakeGenericType(payloadType)
                .GetMethod(nameof(ResultDto<object>.Fail), [typeof(string), typeof(IEnumerable<string>), typeof(ResultFailureKind)])!
                .Invoke(null, ["Test dispatch", null, ResultFailureKind.Validation]);
            return Task.FromResult((TResponse)result!);
        }

        public Task<TResponse> Send<TResponse>(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
