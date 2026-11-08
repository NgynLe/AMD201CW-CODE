namespace Notification.Api.Models;

// Mot thong bao gui cho 1 nguoi dung (hien o chuong thong bao tren app).
// Dat ten UserNotification (khong dat "Notification") de khong trung ten namespace Notification.Api.
public class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }                 // nguoi nhan (claim "sub" trong token)
    public string Type { get; set; } = "";           // heatwave | streak | pump | ...
    public string Title { get; set; } = "";          // noi dung tieng Anh du phong
    public string Body { get; set; } = "";
    public string? DataJson { get; set; }            // tham so cho ban dich, luu dang JSON {"maxTemp":"36"}

    // Chong trung: cung DedupKey chi luu 1 thong bao (vd "heatwave:<userId>:2026-11-08")
    public string? DedupKey { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }            // null = chua doc
}
