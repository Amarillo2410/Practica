using System.Net;
using System.Security.Cryptography;
using Application.Abstractions;
using Application.Abstractions.Auth;
using Application.Common.Exceptions;
using Domain.Entities.Auth;
using MediatR;

namespace Application.UseCase.Auth.EmailVerification;

public sealed class SendEmailVerificationCodeHandler(
    IUnitOfWork unitOfWork,
    IEmailSender emailSender) : IRequestHandler<SendEmailVerificationCodeCommand, SendEmailVerificationCodeResult>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);

    public async Task<SendEmailVerificationCodeResult> Handle(
        SendEmailVerificationCodeCommand request,
        CancellationToken ct)
    {
        var user = await unitOfWork.Users.GetByIdAsync(request.UserId, ct);
        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }

        if (user.IsEmailVerified)
        {
            return new SendEmailVerificationCodeResult
            {
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow,
                AlreadyVerified = true
            };
        }

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var expiresAt = DateTime.UtcNow.Add(CodeLifetime);

        await unitOfWork.EmailVerificationCodes.AddAsync(
            new EmailVerificationCode(user.Id, EmailVerificationCodeHasher.Hash(code), expiresAt),
            ct);
        await unitOfWork.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            user.Email,
            "Tu codigo de verificacion de LinkedIn",
            BuildEmailBody(user.Profile?.FirstName, code),
            ct);

        return new SendEmailVerificationCodeResult
        {
            Email = user.Email,
            ExpiresAt = expiresAt,
            AlreadyVerified = false
        };
    }

    private static string BuildEmailBody(string? firstName, string code)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(firstName) ? "Hola" : firstName.Trim());
        return $"""
            <div style="font-family:Arial,sans-serif;line-height:1.5;color:#1f2328">
              <h2>{safeName}, verifica tu email</h2>
              <p>Usa este codigo para completar tu registro:</p>
              <p style="font-size:28px;font-weight:700;letter-spacing:6px">{code}</p>
              <p>El codigo vence en 10 minutos.</p>
            </div>
            """;
    }
}
