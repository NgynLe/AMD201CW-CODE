using System.Security.Claims;

namespace Garden.Api.Services;

// Doc id nguoi dang goi API tu token (token do Identity service cap).
// Giong cach Forum lam: khong hoi Identity, chu ky token da chung minh token la that.
public static class CurrentUser
{
    public static Guid? Id(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(id, out var guid) ? guid : null;
    }
}
