using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.Dtos.Auth;
using Application.Abstractions;
using Application.Common.Exceptions;
using Application.UseCase.Auth.EmailVerification;
using Application.UseCase.Auth.ExternalLogin;
using Application.UseCase.Auth.LocalLogin;
using Application.UseCase.Auth.RegisterLocal;
using Application.UseCase.Auth.UpdateLocalProfile;
using Domain.Entities.Auth;
using Domain.Enums;
using Infrastructure.Services.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers.Auth;

[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(
    ISender sender,
    IUnitOfWork unitOfWork,
    IWebHostEnvironment environment,
    IOptions<JwtOptions> jwtOptions) : BaseApiController
{
    [HttpPost("register/local")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ExternalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExternalLoginResponse>> RegisterLocal(
        [FromBody] LocalRegisterRequest request,
        CancellationToken ct)
    {
        var command = new RegisterLocalCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Location,
            request.IsStudent,
            request.JobTitle,
            request.Company,
            request.University,
            request.Degree,
            request.Discipline,
            request.StartYear,
            request.JobSearchStatus,
            request.PreferredTitles,
            request.PreferredLocations,
            request.RemoteInterested,
            request.JobAlertsEnabled,
            request.RecruiterVisibility);

        var result = await sender.Send(command, ct);
        return Ok(ToExternalLoginResponse(result));
    }

    [HttpPut("register/local/profile")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OnboardingStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingStatusResponse>> UpdateLocalProfile(
        [FromBody] LocalProfileUpdateRequest request,
        CancellationToken ct)
    {
        var command = new UpdateLocalProfileCommand(
            request.UserId,
            request.FirstName,
            request.LastName,
            request.Location,
            request.IsStudent,
            request.JobTitle,
            request.Company,
            request.University,
            request.Degree,
            request.Discipline,
            request.StartYear,
            request.JobSearchStatus,
            request.PreferredTitles,
            request.PreferredLocations,
            request.RemoteInterested,
            request.JobAlertsEnabled,
            request.RecruiterVisibility,
            request.CompleteOnboarding);

        var result = await sender.Send(command, ct);
        return Ok(new OnboardingStatusResponse
        {
            Completed = result.Completed,
            CurrentStep = result.CurrentStep
        });
    }

    [HttpPost("email-verification/send")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SendEmailVerificationCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SendEmailVerificationCodeResponse>> SendEmailVerificationCode(
        [FromBody] SendEmailVerificationCodeRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new SendEmailVerificationCodeCommand(request.UserId, request.Email), ct);
        return Ok(new SendEmailVerificationCodeResponse
        {
            Email = result.Email,
            ExpiresAt = result.ExpiresAt,
            AlreadyVerified = result.AlreadyVerified,
            CodeSent = result.CodeSent,
            Message = result.Message,
            PreviewCode = environment.IsDevelopment() ? result.PreviewCode : null
        });
    }

    [HttpPost("email-verification/verify")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(VerifyEmailCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VerifyEmailCodeResponse>> VerifyEmailCode(
        [FromBody] VerifyEmailCodeRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new VerifyEmailCodeCommand(request.UserId, request.Email, request.Code), ct);
        return Ok(new VerifyEmailCodeResponse
        {
            EmailVerified = result.EmailVerified,
            Onboarding = new OnboardingStatusResponse
            {
                Completed = result.OnboardingCompleted,
                CurrentStep = result.CurrentOnboardingStep
            }
        });
    }

    [HttpPost("register/local/profile-photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(5_242_880)]
    [ProducesResponseType(typeof(ProfilePhotoUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProfilePhotoUploadResponse>> UploadProfilePhoto(
        [FromForm] ProfilePhotoUploadRequest request,
        CancellationToken ct)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var targetUserId = request.UserId == Guid.Empty ? authenticatedUserId : request.UserId;

        if (targetUserId != authenticatedUserId)
        {
            throw new UnauthorizedException("You can only update your own profile photo.");
        }

        if (request.File is null || request.File.Length == 0)
        {
            throw new BadRequestException("Profile photo is required.");
        }

        var extension = ResolveImageExtension(request.File.ContentType, request.File.FileName);
        if (extension is null)
        {
            throw new BadRequestException("Only image files are allowed.");
        }

        var user = await unitOfWork.Users.GetByIdAsync(targetUserId, ct);
        if (user is null)
        {
            throw new BadRequestException("User not found.");
        }

        var webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;
        var uploadDirectory = Path.Combine(webRootPath, "uploads", "profile-pictures");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{user.Id:N}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await request.File.CopyToAsync(stream, ct);
        }

        var profilePicturePath = $"/uploads/profile-pictures/{fileName}";
        var profile = user.Profile ?? new UserProfile(user.Id, firstName: null, lastName: null, avatarUrl: null);
        profile.UpdateAvatar(profilePicturePath);
        user.SetProfile(profile);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok(new ProfilePhotoUploadResponse { ProfilePictureUrl = BuildAbsoluteMediaUrl(profilePicturePath) });
    }

    [HttpDelete("register/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelIncompleteRegistration(CancellationToken ct)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var user = await unitOfWork.Users.GetByIdAsync(authenticatedUserId, ct);

        if (user is null)
        {
            return NoContent();
        }

        if (user.OnboardingComplete)
        {
            throw new BadRequestException("Completed accounts cannot be cancelled from onboarding.");
        }

        await unitOfWork.Users.DeleteAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("login/local")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ExternalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExternalLoginResponse>> LocalLogin(
        [FromBody] LocalLoginRequest request,
        CancellationToken ct)
    {
        var command = new LocalLoginCommand(request.Email, request.Password);
        var result = await sender.Send(command, ct);
        return Ok(ToExternalLoginResponse(result));
    }

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
        return Ok(ToExternalLoginResponse(result));
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
        return Ok(ToExternalLoginResponse(result));
    }

    private static string? ResolveImageExtension(string? contentType, string? fileName)
    {
        var extension = contentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/avif" => ".avif",
            "image/heic" => ".heic",
            "image/heif" => ".heif",
            _ => null
        };

        if (extension is not null)
        {
            return extension;
        }

        var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return fileExtension is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".bmp" or ".avif" or ".heic" or ".heif"
            ? fileExtension
            : null;
    }

    private Guid GetAuthenticatedUserId()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedException("Access token is required.");
        }

        var accessToken = authorization["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new UnauthorizedException("Access token is required.");
        }

        var options = jwtOptions.Value;
        var tokenHandler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        try
        {
            var principal = tokenHandler.ValidateToken(
                accessToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(options.Issuer),
                    ValidIssuer = options.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                },
                out _);

            var userIdValue = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : throw new UnauthorizedException("Access token does not contain a valid user id.");
        }
        catch (Exception ex) when (ex is not UnauthorizedException)
        {
            throw new UnauthorizedException("Invalid access token.");
        }
    }

    private string BuildAbsoluteMediaUrl(string path)
        => $"{Request.Scheme}://{Request.Host}{path}";

    private static ExternalLoginResponse ToExternalLoginResponse(ExternalLoginResult result)
    {
        return new ExternalLoginResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            IsNewUser = result.IsNewUser,
            VerificationCodeSent = result.VerificationCodeSent,
            VerificationMessage = result.VerificationMessage,
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
