using Identity.Api.Data;
using Identity.Api.DTOs;
using Identity.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Services;

// Loi nghiep vu co thong bao than thien de tra ve cho app
public class AuthException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class AuthService(IdentityDbContext db, JwtTokenService jwt)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new AuthException("This email is already registered. Log in or use another email.", 409);

        var user = new User
        {
            Email = email,
            DisplayName = req.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = Roles.User,   // Dang ky cong khai luon la User, Admin chi do he thong tao
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Cung mot thong bao cho ca 2 truong hop de khong lo email nao da ton tai
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new AuthException("Incorrect email or password.", 401);

        return await IssueTokensAsync(user);
    }

    // Dung refresh token cu de lay cap token moi (token cu bi thu hoi)
    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var stored = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (stored is null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
            throw new AuthException("Your session has expired. Please log in again.", 401);

        stored.RevokedAt = DateTime.UtcNow;
        return await IssueTokensAsync(stored.User);
    }

    // Dang xuat: thu hoi refresh token, khong lam moi duoc nua
    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (stored is not null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    // Doi mat khau: phai dung mat khau cu. Doi xong thu hoi TAT CA refresh token
    // (cac may khac dang dang nhap se bi dang xuat), roi cap token moi cho may hien tai.
    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest req)
    {
        var user = await db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new AuthException("Account not found.", 404);

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            throw new AuthException("Your current password is incorrect.", 400);

        if (req.CurrentPassword == req.NewPassword)
            throw new AuthException("Your new password must be different from the current one.", 400);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        var now = DateTime.UtcNow;
        foreach (var token in user.RefreshTokens.Where(t => t.RevokedAt == null))
            token.RevokedAt = now;

        await db.SaveChangesAsync();
        return await IssueTokensAsync(user);
    }

    // Cap access token + refresh token moi (dung chung cho dang nhap, dang ky, doi/dat lai mat khau)
    public async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var (accessToken, expiresAt) = jwt.CreateAccessToken(user);
        var refresh = jwt.CreateRefreshToken(user.Id);

        db.RefreshTokens.Add(refresh);
        await db.SaveChangesAsync();

        return new AuthResponse(accessToken, refresh.Token, expiresAt, ToDto(user));
    }

    public static UserDto ToDto(User u) => new(u.Id, u.Email, u.DisplayName, u.Role);
}
