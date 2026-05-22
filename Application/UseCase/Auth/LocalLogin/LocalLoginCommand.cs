using Application.UseCase.Auth.ExternalLogin;
using MediatR;

namespace Application.UseCase.Auth.LocalLogin;

public sealed record LocalLoginCommand(string Email, string Password) : IRequest<ExternalLoginResult>;
