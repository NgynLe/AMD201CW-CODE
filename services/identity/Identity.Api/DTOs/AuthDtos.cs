using System.ComponentModel.DataAnnotations;

namespace Identity.Api.DTOs;

// Du lieu app gui len
public record RegisterRequest(
    [Required, StringLength(50, MinimumLength = 2)] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshRequest([Required] string RefreshToken);

public record ChangeRoleRequest([Required] string Role);

public record ForgotPasswordRequest([Required, EmailAddress] string Email);

public record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Code,
    [Required, MinLength(8)] string NewPassword);

public record MessageResponse(string Message);

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword);

// Du lieu tra ve cho app
public record UserDto(Guid Id, string Email, string DisplayName, string Role);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record ErrorResponse(string Message);
