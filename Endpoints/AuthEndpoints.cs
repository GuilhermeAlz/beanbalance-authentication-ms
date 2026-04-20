using System.Security.Claims;
using AuthMicroservice.DTOs;
using AuthMicroservice.Services;

namespace AuthMicroservice.Endpoints;

public static class AuthEndpoints
{
    private static string? GetClaimValue(ClaimsPrincipal user, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }

        return null;
    }

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Created($"/api/auth/{response.Username}", response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .WithName("Register")
        .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("Login")
        .AllowAnonymous();

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            try
            {
                var response = await authService.RefreshTokenAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("RefreshToken")
        .AllowAnonymous();

        group.MapPost("/revoke", async (ClaimsPrincipal user, IAuthService authService) =>
        {
            // ClaimsPrincipal é injetado automaticamente pelo middleware de Authentication
            // Contém as claims do JWT decodificado
            var email = GetClaimValue(user, "email", ClaimTypes.Email);
            if (email is null) return Results.Unauthorized();

            await authService.RevokeTokenAsync(email);
            return Results.NoContent();
        })
        .WithName("RevokeToken")
        .RequireAuthorization();

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                Id = GetClaimValue(user, "sub", ClaimTypes.NameIdentifier),
                Username = GetClaimValue(user, "username", ClaimTypes.Name),
                Email = GetClaimValue(user, "email", ClaimTypes.Email),
                Role = GetClaimValue(user, "role", ClaimTypes.Role)
            });
        })
        .WithName("GetCurrentUser")
        .RequireAuthorization();
    }
}
