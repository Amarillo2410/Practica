using MediatR;

namespace Application.UseCase.Auth.EmailVerification;

public sealed record VerifyEmailCodeCommand(Guid? UserId, string? Email, string Code) : IRequest<VerifyEmailCodeResult>;
