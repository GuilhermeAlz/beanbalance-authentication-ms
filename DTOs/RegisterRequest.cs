using System.ComponentModel.DataAnnotations;

namespace AuthMicroservice.DTOs;

public record RegisterRequest(
    [Required] [MinLength(3)] string Username,
    [Required] [EmailAddress] string Email,
    [Required] [MinLength(6)] string Password
);
