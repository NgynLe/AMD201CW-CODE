namespace Garden.Api.Models;

// 1 dong = 1 ngay nguoi dung da tuoi cay (nhieu lan trong ngay van chi 1 dong).
// Chuoi tuoi cay = so ngay lien tiep co dong trong bang nay.
public class WateringDay
{
    public Guid UserId { get; set; }                 // claim "sub" trong token
    public DateOnly Date { get; set; }               // ngay theo gio Viet Nam (UTC+7), xem Streak:UtcOffsetHours
    public int Count { get; set; } = 1;              // so lan tuoi trong ngay do
    public DateTime LastWateredAtUtc { get; set; } = DateTime.UtcNow;
}
