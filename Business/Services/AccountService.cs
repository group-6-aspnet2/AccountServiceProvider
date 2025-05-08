﻿using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Contracts;

namespace Business.Services;

public class AccountService(UserManager<IdentityUser> userManager) : AccountGrpcService.AccountGrpcServiceBase
{
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public override async Task<CreateAccountReply> CreateAccount(CreateAccountRequest request, ServerCallContext context)
    {
        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        var reply = new CreateAccountReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
                ? "Account was created successfully."
                : string.Join(", ", result.Errors.Select(e => e.Description))
        };

        if (result.Succeeded)
            reply.UserId = user.Id;

        return reply;
    }

    public override async Task<ValidateCredentialsReply> ValidateCredentials(ValidateCredentialsRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new ValidateCredentialsReply
            {
                Succeeded = false,
                Message = "Email and password must be provided."
            };
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new ValidateCredentialsReply
            {
                Succeeded = false,
                Message = "Invalid credentials."
            };
        }

        return new ValidateCredentialsReply
        {
            Succeeded = true,
            Message = "Login successful.",
            UserId = user.Id
        };
    }

    public override async Task<GetAccountsReply> GetAccounts(GetAccountsRequest request, ServerCallContext context)
    {
        var users = await _userManager.Users.ToListAsync();

        var reply = new GetAccountsReply
        {
            Succeeded = true,
            Message = users.Count > 0 ? "Accounts retrived successfully." : "No Accounts found."
        };

        foreach (var user in users)
        {
            reply.Accounts.Add(new Account
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }

        return reply;
    }

    public override async Task<GetAccountByIdReply> GetAccountById(GetAccountByIdRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new GetAccountByIdReply { Succeeded = false, Message = "No account found." };

        var account = new Account
        {
            UserId = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
        };

        return new GetAccountByIdReply { Succeeded = true, Account = account, Message = "Account was found." };

    }

    public override async Task<UpdatePhoneNumberReply> UpdatePhoneNumber(UpdatePhoneNumberRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new UpdatePhoneNumberReply
            {
                Succeeded = false,
                Message = "No account found."
            };
        }

        if (!string.Equals(user.PhoneNumber, request.PhoneNumber, StringComparison.Ordinal))
            user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        return new UpdatePhoneNumberReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
            ? "Account was updated successfully."
            : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<DeleteAccountByIdReply> DeleteAccountById(DeleteAccountByIdRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new DeleteAccountByIdReply
            {
                Succeeded = false,
                Message = "No accound found."
            };
        }

        var result = await _userManager.DeleteAsync(user);

        return new DeleteAccountByIdReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
            ? "Account was deleted successfully."
            : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<ConfirmAccountReply> ConfirmAccount(ConfirmAccountRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByEmailAsync(request.UserId);
        if (user == null)
        {
            return new ConfirmAccountReply
            {
                Succeeded = false,
                Message = "No account found."
            };
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            return new ConfirmAccountReply
            {
                Succeeded = true,
                Message = "Account is already confirmed."
            };
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        return new ConfirmAccountReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
            ? "Email confirmed successfully."
            : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<ConfirmEmailChangeReply> ConfirmEmailChange(ConfirmEmailChangeRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new ConfirmEmailChangeReply
            {
                Succeeded = false,
                Message = "User not found"
            };
        }

        var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, request.Token);

        return new ConfirmEmailChangeReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
            ? "Email address updated successfully."
            : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<UpdateEmailReply> UpdateEmail(UpdateEmailRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByEmailAsync(request.UserId);
        if (user == null)
        {
            return new UpdateEmailReply
            {
                Succeeded = false,
                Message = "User not found."
            };
        }

        if (string.Equals(user.Email, request.NewEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new UpdateEmailReply
            {
                Succeeded = true,
                Message = "Email is already up to date.",
                Token = string.Empty
            };
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);

        return new UpdateEmailReply
        {
            Succeeded = true,
            Message = "Token generated for email change.",
            Token = token
        };
    }

    public override async Task<ResetPasswordReply> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return new ResetPasswordReply
            {
                Succeeded = false,
                Message = "User not found"
            };
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return new ResetPasswordReply
        {
            Succeeded = result.Succeeded,
            Message = result.Succeeded
            ? "Password has been reset successfully."
            : string.Join(", ", result.Errors.Select(e => e.Description))
        };
    }

    public override async Task<GenerateTokenReply> GenerateEmailConfirmationToken(GenerateTokenRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByEmailAsync(request.UserId);
        if (user == null)
        {
            return new GenerateTokenReply
            {
                Succeeded = false,
                Message = "No account found."
            };
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        return new GenerateTokenReply
        {
            Succeeded = true,
            Token = token,
            Message = "Token generated successfully."
        };
    }

    public override async Task<GenerateTokenReply> GeneratePasswordResetToken(GenerateTokenRequest request, ServerCallContext context)
    {
        var user = await _userManager.FindByEmailAsync(request.UserId);
        if (user == null)
        {
            return new GenerateTokenReply
            {
                Succeeded = false,
                Message = "User not found."
            };
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return new GenerateTokenReply
        {
            Succeeded = true,
            Token = token,
            Message = "Password reset token generated."
        };
    }
}