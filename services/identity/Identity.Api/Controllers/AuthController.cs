using System.Security.Claims;
using Identity.Api.Data;
using Identity.Api.DTOs;
using Identity.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth, PasswordResetService passwordReset, IdentityDbContext db) : ControllerBase
{
    // POST /api/auth/forgot-password   { "email": "..." }
    // Luon tra ve cung mot cau, du email co ton tai hay khong (khong lo thong tin)
    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest req)
    {
        await passwordReset.RequestAsync(req.Email);
        return Ok(new MessageResponse("If this email is registered, a reset code has been sent to it."));
    }

    // POST /api/auth/reset-password   { "email": "...", "code": "123456", "newPassword": "..." }
    // Dung ma -> doi mat khau va tra ve token moi (dang nhap luon)
    [HttpPost("reset-password")]
    public async Task<ActionResult<AuthResponse>> ResetPassword(ResetPasswordRequest req)
    {
        try { return Ok(await passwordReset.ResetAsync(req)); }
        catch (AuthException ex) { return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message)); }
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        try { return Ok(await auth.RegisterAsync(req)); }
        catch (AuthException ex) { return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message)); }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        try { return Ok(await auth.LoginAsync(req)); }
        catch (AuthException ex) { return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message)); }
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest req)
    {
        try { return Ok(await auth.RefreshAsync(req.RefreshToken)); }
        catch (AuthException ex) { return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message)); }
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest req)
    {
        await auth.LogoutAsync(req.RefreshToken);
        return NoContent();
    }

    // GET /api/auth/me  (phai gui kem token)
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        if (CurrentUserId() is not Guid userId) return Unauthorized();

        var user = await db.Users.FindAsync(userId);
        return user is null ? NotFound() : Ok(AuthService.ToDto(user));
    }

    // PUT /api/auth/password  (phai gui kem token)
    [Authorize]
    [HttpPut("password")]
    public async Task<ActionResult<AuthResponse>> ChangePassword(ChangePasswordRequest req)
    {
        if (CurrentUserId() is not Guid userId) return Unauthorized();

        try { return Ok(await auth.ChangePasswordAsync(userId, req)); }
        catch (AuthException ex) { return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message)); }
    }

    // Lay id nguoi dung tu token
    private Guid? CurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(id, out var userId) ? userId : null;
    }
}
