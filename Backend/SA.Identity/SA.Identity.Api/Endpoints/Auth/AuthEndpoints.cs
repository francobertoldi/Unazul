using Mediator;
using SA.Identity.Api.Mappers.Auth;
using SA.Identity.Api.ViewModels.Auth.Login;
using SA.Identity.Api.ViewModels.Auth.Logout;
using SA.Identity.Api.ViewModels.Auth.Otp;
using SA.Identity.Api.ViewModels.Auth.Password;
using SA.Identity.Api.ViewModels.Auth.Refresh;
using SA.Identity.Application.Commands.Auth;
using Shared.Auth;
using Shared.Contract.Exceptions;
using Shared.Contract.Models;

namespace SA.Identity.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .WithOpenApi();

        // RF-SEC-01: Login
        group.MapPost("/login", async (
            LoginRequest request,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            // For anonymous endpoints, read tenant from X-Tenant-Id header
            var tenantId = GetTenantIdFromHeader(httpContext);

            var result = await mediator.Send(new LoginCommand(
                tenantId,
                request.Username,
                request.Password));

            return Results.Ok(AuthMapper.ToLoginResponse(result));
        })
        .AllowAnonymous()
        .Produces<LoginResponse>(200);

        // RF-SEC-02: Verify OTP
        group.MapPost("/otp/verify", async (
            VerifyOtpRequest request,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new VerifyOtpCommand(
                Guid.Empty, // TenantId resolved from user in handler
                request.UserId,
                request.OtpCode));

            return Results.Ok(AuthMapper.ToVerifyOtpResponse(result));
        })
        .AllowAnonymous()
        .Produces<VerifyOtpResponse>(200);

        // RF-SEC-02b: Resend OTP
        group.MapPost("/otp/resend", async (
            ResendOtpRequest request,
            IMediator mediator) =>
        {
            await mediator.Send(new ResendOtpCommand(request.UserId));
            return Results.Ok(new { Message = "Nuevo codigo OTP enviado." });
        })
        .AllowAnonymous()
        .Produces(200);

        // RF-SEC-03: Refresh token
        group.MapPost("/refresh", async (
            RefreshTokenRequest request,
            IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new RefreshTokenCommand(request.RefreshToken));
                return Results.Ok(AuthMapper.ToRefreshTokenResponse(result));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(
                    new ErrorResponse(ex.Message, "AUTH_TOKEN_INVALID"),
                    statusCode: 401);
            }
        })
        .AllowAnonymous()
        .Produces<RefreshTokenResponse>(200)
        .Produces<ErrorResponse>(401);

        // RF-SEC-04: Forgot password
        group.MapPost("/recover-password", async (
            ForgotPasswordRequest request,
            HttpContext httpContext,
            IMediator mediator) =>
        {
            var tenantId = GetTenantIdFromHeader(httpContext);

            await mediator.Send(new ForgotPasswordCommand(
                tenantId,
                request.Email));

            // Always return 200 to prevent user enumeration
            return Results.Ok(new { Message = "Si el email existe, se enviara un enlace de recuperacion." });
        })
        .AllowAnonymous()
        .Produces(200);

        // RF-SEC-04b: Reset password
        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            IMediator mediator) =>
        {
            await mediator.Send(new ResetPasswordCommand(
                request.Token,
                request.NewPassword));

            return Results.Ok(new { Message = "Contrasena actualizada correctamente." });
        })
        .AllowAnonymous()
        .Produces(200);

        // RF-SEC-05: Logout
        group.MapPost("/logout", async (
            LogoutRequest request,
            IMediator mediator,
            ICurrentUser currentUser) =>
        {
            await mediator.Send(new LogoutCommand(
                currentUser.UserId,
                request.RefreshToken));

            return Results.Ok(new { Message = "Sesion cerrada correctamente." });
        })
        .RequireAuthorization()
        .Produces(200);
    }

    private static Guid GetTenantIdFromHeader(HttpContext httpContext)
    {
        var headerValue = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (Guid.TryParse(headerValue, out var tenantId))
        {
            return tenantId;
        }

        // Fallback: try from JWT claim (for authenticated requests)
        var claimValue = httpContext.User.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(claimValue, out var claimTenantId))
        {
            return claimTenantId;
        }

        throw new ValidationException("AUTH_TENANT_REQUIRED", "El header X-Tenant-Id es requerido.");
    }
}
