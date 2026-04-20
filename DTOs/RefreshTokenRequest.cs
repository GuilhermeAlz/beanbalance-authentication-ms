namespace AuthMicroservice.DTOs;

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
