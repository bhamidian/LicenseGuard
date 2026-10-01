using FluentValidation;
using LicenseGuard.Application.Contracts;
using LicenseGuard.Application.Features.Auth.Commands.RequestLoginCode;
using LicenseGuard.Application.Features.Auth.Records;
using LicenseGuard.Domain.Dtos;
using LicenseGuard.Domain.Entities;
using LicenseGuard.Domain.Records;
using LicenseGuard.Domain.Repositories;
using LicenseGuard.Infrastructure.JwtService.Contracts;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LicenseGuard.Application.Features.Auth.Commands.VerifyLoginCode;

public sealed class VerifyLoginCodeCommandHandler(
    IValidator<VerifyLoginCodeCommand> validator,
    UserManager<AppUser> users,
    RoleManager<IdentityRole<Guid>> roles,
    IAppUserRepository appUsers,
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokens)
    : IRequestHandler<VerifyLoginCodeCommand, ResultDto<VerifyLoginCodeResponse>>
{
    private const string TokenPurpose = "LicenseGuard.Login";
    private const string CustomerRole = "Customer";

    public async Task<ResultDto<VerifyLoginCodeResponse>> Handle(
        VerifyLoginCodeCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            return ResultDto<VerifyLoginCodeResponse>.Fail("Login verification request is invalid.",
                validation.Errors.Select(error => error.ErrorMessage));

        await using var transaction = await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        var contact = RequestLoginCodeCommandHandler.Normalize(command.Channel, command.Contact);
        var user = command.Channel switch
        {
            LoginCodeChannel.Email => await users.FindByEmailAsync(contact),
            LoginCodeChannel.Sms => await appUsers.FindByPhoneNumberAsync(contact, cancellationToken),
            _ => null
        };

        if (user is null || await users.IsLockedOutAsync(user))
            return InvalidCode();

        var provider = command.Channel == LoginCodeChannel.Email
            ? TokenOptions.DefaultEmailProvider
            : TokenOptions.DefaultPhoneProvider;
        var isValidCode = await users.VerifyUserTokenAsync(user, provider, TokenPurpose, command.Code);
        if (!isValidCode)
        {
            if (!user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
                await users.UpdateAsync(user);
            }
            await users.AccessFailedAsync(user);
            await transaction.CommitAsync(cancellationToken);
            return InvalidCode();
        }

        await users.ResetAccessFailedCountAsync(user);
        if (command.Channel == LoginCodeChannel.Email) user.EmailConfirmed = true;
        else user.PhoneNumberConfirmed = true;
        var updateResult = await users.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ResultDto<VerifyLoginCodeResponse>.Fail("Could not confirm the login contact.");

        var userRoles = (await users.GetRolesAsync(user)).ToList();
        if (userRoles.Count == 0)
        {
            if (!await roles.RoleExistsAsync(CustomerRole))
            {
                var createRoleResult = await roles.CreateAsync(new IdentityRole<Guid>(CustomerRole) { Id = Guid.NewGuid() });
                if (!createRoleResult.Succeeded)
                    return ResultDto<VerifyLoginCodeResponse>.Fail("Could not initialize the customer role.");
            }

            var addRoleResult = await users.AddToRoleAsync(user, CustomerRole);
            if (!addRoleResult.Succeeded)
                return ResultDto<VerifyLoginCodeResponse>.Fail("Could not assign the customer role.");
            userRoles.Add(CustomerRole);
        }

        if (userRoles.Contains(CustomerRole, StringComparer.OrdinalIgnoreCase)
            && await customers.GetByAppUserIdAsync(user.Id, cancellationToken) is null)
            customers.Create(new CreateCustomerProfileRecord(user.Id));

        // Rotating the stamp makes a successful six-digit code unusable for a second login.
        var stampResult = await users.UpdateSecurityStampAsync(user);
        if (!stampResult.Succeeded)
            return ResultDto<VerifyLoginCodeResponse>.Fail("Could not finalize login.");

        var token = jwtTokens.CreateAccessToken(new JwtTokenRequest(user.Id, user.UserName ?? contact,
            userRoles, user.Email));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ResultDto<VerifyLoginCodeResponse>.Success("Login successful.",
            new VerifyLoginCodeResponse(user.Id, userRoles, token));
    }

    private static ResultDto<VerifyLoginCodeResponse> InvalidCode() =>
        ResultDto<VerifyLoginCodeResponse>.Fail("The login code is invalid or expired.",
            failureKind: ResultFailureKind.Unauthorized);
}
