using Api.Dtos.Auth;
using Application.UseCase.Auth.ExternalLogin;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Auth;

[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(ISender sender) : BaseApiController
{
    [HttpPost("external-login/google")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ExternalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExternalLoginResponse>> GoogleExternalLogin(
        [FromBody] GoogleExternalLoginRequest request,
        CancellationToken ct)
    {
        var command = new ExternalLoginCommand(AuthProvider.Google, request.IdToken);
        var result = await sender.Send(command, ct);
        return Ok(ToResponse(result));
    }

    [HttpPost("external-login/microsoft")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ExternalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExternalLoginResponse>> MicrosoftExternalLogin(
        [FromBody] MicrosoftExternalLoginRequest request,
        CancellationToken ct)
    {
        var command = new ExternalLoginCommand(AuthProvider.Microsoft, request.IdToken);
        var result = await sender.Send(command, ct);
        return Ok(ToResponse(result));
    }

    private static ExternalLoginResponse ToResponse(ExternalLoginResult result)
    {
        return new ExternalLoginResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            IsNewUser = result.IsNewUser,
            User = new AuthUserResponse
            {
                Id = result.User.Id,
                Email = result.User.Email,
                FirstName = result.User.FirstName,
                LastName = result.User.LastName,
                ProfilePictureUrl = result.User.ProfilePictureUrl
            },
            Onboarding = new OnboardingStatusResponse
            {
                Completed = result.Onboarding.Completed,
                CurrentStep = result.Onboarding.CurrentStep
            }
        };
    }
}
