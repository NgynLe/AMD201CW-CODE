using System.Security.Claims;

namespace AiAssistant.Api.Services;

// Doc thong tin nguoi dang goi API tu token (token do Identity service cap).
public static class CurrentUser
{
    public static Guid? Id(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    public static string? Name(ClaimsPrincipal user) =>
        user.FindFirstValue("name") ?? user.FindFirstValue(ClaimTypes.Name);
}
