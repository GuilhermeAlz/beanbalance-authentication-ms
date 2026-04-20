namespace AuthMicroservice.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime Expiration,
    string Username,
    string Role
);
