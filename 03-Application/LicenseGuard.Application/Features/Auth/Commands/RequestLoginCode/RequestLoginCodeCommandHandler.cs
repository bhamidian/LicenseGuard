using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.Auth.Records;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;

public sealed class RequestLoginCodeCommandHandler(
    IValidator<RequestLoginCodeCommand> validator,
    UserManager<AppUser> users,
    IAppUserRepository appUsers,
    IEmailOtpSender? emailSender = null,
    ISmsOtpSender? smsSender = null)
    : IRequestHandler<RequestLoginCodeCommand, ResultDto<RequestLoginCodeResponse>>
{
    private const string TokenPurpose = "LicenseGuard.Login";

    public async Task<ResultDto<RequestLoginCodeResponse>> Handle(
        RequestLoginCodeCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<RequestLoginCodeResponse>.Fail("Login code request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        if (command.Channel == LoginCodeChannel.Email && emailSender is null
            || command.Channel == LoginCodeChannel.Sms && smsSender is null)
            return ResultDto<RequestLoginCodeResponse>.Fail("The selected login-code delivery provider is not configured.",
                failureKind: ResultFailureKind.Unavailable);

        var contact = Normalize(command.Channel, command.Contact);
        var user = command.Channel switch
        {
            LoginCodeChannel.Email => await users.FindByEmailAsync(contact),
            LoginCodeChannel.Sms => await appUsers.FindByPhoneNumberAsync(contact, cancellationToken),
            _ => null
        };

        if (user is null)
        {
            user = await appUsers.CreatePendingAsync(command.Channel == LoginCodeChannel.Email
                ? new CreatePendingAppUserRecord(contact, contact, null)
                : new CreatePendingAppUserRecord(contact, null, contact), cancellationToken);
        }

        if (user is not null)
        {
            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                await users.UpdateAsync(user);
            }
            var provider = command.Channel == LoginCodeChannel.Email
                ? TokenOptions.DefaultEmailProvider
                : TokenOptions.DefaultPhoneProvider;
            var code = await users.GenerateUserTokenAsync(user, provider, TokenPurpose);
            if (command.Channel == LoginCodeChannel.Email)
                await emailSender!.SendAsync(contact, code, cancellationToken);
            else
                await smsSender!.SendAsync(contact, code, cancellationToken);
        }

        return ResultDto<RequestLoginCodeResponse>.Success(
            "If the contact is eligible, a login code has been sent.",
            new RequestLoginCodeResponse("If the contact is eligible, a login code has been sent."));
    }

    internal static string Normalize(LoginCodeChannel channel, string contact) =>
        channel == LoginCodeChannel.Email ? contact.Trim().ToLowerInvariant() : contact.Trim();
}
