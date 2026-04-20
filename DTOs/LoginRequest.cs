using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.DTOs;

public record LoginRequest(
    [Required] string Email,
    [Required] string Password
);
