using AuthMicroservice.Data;
using AuthMicroservice.DTOs;
using AuthMicroservice.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthMicroservice.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, ITokenService tokenService, IConfiguration config)
    {
        _context = context;
        _tokenService = tokenService;
        _config = config;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email já cadastrado");

        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Username já cadastrado");

        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RefreshToken = HashToken(rawRefreshToken),
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
        };

        // Add + SaveChangesAsync = equivalente ao save() do JpaRepository
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_config["Jwt:ExpirationMinutes"]!));

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            expiration,
            user.Username,
            user.Role
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciais inválidas");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = HashToken(rawRefreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_config["Jwt:ExpirationMinutes"]!));

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            expiration,
            user.Username,
            user.Role
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var email = principal?.FindFirst("email")?.Value
            ?? principal?.FindFirst(ClaimTypes.Email)?.Value;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || user.RefreshToken != HashToken(request.RefreshToken)
            || user.RefreshTokenExpiry <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado");

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRawRefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = HashToken(newRawRefreshToken);
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync();

        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_config["Jwt:ExpirationMinutes"]!));

        return new AuthResponse(
            newAccessToken,
            newRawRefreshToken,
            expiration,
            user.Username,
            user.Role
        );
    }

    public async Task RevokeTokenAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) throw new InvalidOperationException("Usuário não encontrado");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();
    }
}
