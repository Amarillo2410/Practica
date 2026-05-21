using MediatR;

namespace Application.UseCase.Auth.EmailVerification;

public sealed record SendEmailVerificationCodeCommand(Guid? UserId, string? Email) : IRequest<SendEmailVerificationCodeResult>;
