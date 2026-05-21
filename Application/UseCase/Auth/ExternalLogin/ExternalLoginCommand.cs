using Domain.Enums;
using MediatR;

namespace Application.UseCase.Auth.ExternalLogin;

public sealed record ExternalLoginCommand(AuthProvider Provider, string IdToken) : IRequest<ExternalLoginResult>;
