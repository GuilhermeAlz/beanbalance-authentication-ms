using AuthMicroservice.DTOs;

namespace AuthMicroservice.Services;

public interface IAuthService
{
    /// <summary>
    /// Registra um novo usuário e retorna tokens de acesso.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Autentica um usuário existente e retorna novos tokens.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Renova o Access Token usando um Refresh Token válido.
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>
    /// Invalida o Refresh Token do usuário (logout efetivo).
    /// </summary>
    Task RevokeTokenAsync(string email);
}
