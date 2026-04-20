using System.Security.Claims;
using AuthMicroservice.Models;

namespace AuthMicroservice.Services;

public interface ITokenService
{
    /// <summary>
    /// Gera um JWT (Access Token) contendo as claims do usuário.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Gera um Refresh Token aleatório (string opaca, não é JWT).
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Extrai as claims de um Access Token expirado (para refresh).
    /// Retorna null se o token for inválido.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
