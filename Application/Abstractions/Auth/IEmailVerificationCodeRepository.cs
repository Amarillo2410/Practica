using Domain.Entities.Auth;

namespace Application.Abstractions.Auth;

public interface IEmailVerificationCodeRepository
{
    Task<EmailVerificationCode?> GetLatestActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(EmailVerificationCode code, CancellationToken ct = default);
}
