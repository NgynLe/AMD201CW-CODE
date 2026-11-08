namespace Notification.Api.DTOs;

// Du lieu tra ve cho app
public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    Dictionary<string, string>? Data,
    DateTime CreatedAt,
    bool IsRead);

public record NotificationListDto(int UnreadCount, List<NotificationDto> Items);

// App doc loi tu truong "message" (giong Identity va Forum)
public record ErrorResponse(string Message);
