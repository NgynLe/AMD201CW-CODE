using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification.Api.Data;
using Notification.Api.DTOs;
using Notification.Api.Messaging;
using Notification.Api.Models;
using Notification.Api.Services;

namespace Notification.Api.Controllers;

// Moi API o day deu can dang nhap (gui kem token). Nguoi dung chi thay thong bao cua chinh minh.
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(NotificationDbContext db) : ControllerBase
{
    // GET /api/notifications?take=30   -> thong bao moi nhat + so thong bao chua doc
    [HttpGet]
    public async Task<ActionResult<NotificationListDto>> List([FromQuery] int take = 30)
    {
        if (CurrentUser.Id(User) is not Guid me) return Unauthorized();
        take = Math.Clamp(take, 1, 100);

        var unread = await db.Notifications.CountAsync(n => n.UserId == me && n.ReadAt == null);
        var rows = await db.Notifications
            .Where(n => n.UserId == me)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

        return Ok(new NotificationListDto(unread, rows.Select(ToDto).ToList()));
    }

    // POST /api/notifications/{id}/read   -> danh dau da doc 1 thong bao
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        if (CurrentUser.Id(User) is not Guid me) return Unauthorized();

        // Loc theo UserId: nguoi khac khong danh dau duoc thong bao cua minh (404, khong lo la no ton tai)
        var item = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == me);
        if (item is null) return NotFound(new ErrorResponse("Notification not found."));

        if (item.ReadAt is null)
        {
            item.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        return NoContent();
    }

    // POST /api/notifications/read-all   -> danh dau da doc tat ca
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        if (CurrentUser.Id(User) is not Guid me) return Unauthorized();

        var now = (DateTime?)DateTime.UtcNow;
        await db.Notifications
            .Where(n => n.UserId == me && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, now));
        return NoContent();
    }

    // DELETE /api/notifications/{id}   -> xoa 1 thong bao cua minh
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (CurrentUser.Id(User) is not Guid me) return Unauthorized();

        var item = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == me);
        if (item is null) return NotFound(new ErrorResponse("Notification not found."));

        db.Notifications.Remove(item);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static NotificationDto ToDto(UserNotification n)
    {
        Dictionary<string, string>? data = null;
        if (!string.IsNullOrEmpty(n.DataJson))
        {
            try
            {
                data = JsonSerializer.Deserialize<Dictionary<string, string>>(n.DataJson, NotificationMessageParser.JsonOptions);
            }
            catch (JsonException)
            {
                // Du lieu tham so hong thi bo qua, app van hien Title/Body
            }
        }
        return new NotificationDto(n.Id, n.Type, n.Title, n.Body, data, n.CreatedAt, n.ReadAt is not null);
    }
}
