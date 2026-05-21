using MediatR;

namespace Application.UseCase.Auth.EmailVerification;

public sealed record VerifyEmailCodeCommand(Guid UserId, string Code) : IRequest<VerifyEmailCodeResult>;
