using Microsoft.EntityFrameworkCore;
using Notification.Api.Models;

namespace Notification.Api.Data;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<UserNotification> Notifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserNotification>(e =>
        {
            e.ToTable("Notifications");
            e.Property(n => n.Type).HasMaxLength(NotificationLimits.Type);
            e.Property(n => n.Title).HasMaxLength(NotificationLimits.Title);
            e.Property(n => n.Body).HasMaxLength(NotificationLimits.Body);
            e.Property(n => n.DedupKey).HasMaxLength(NotificationLimits.DedupKey);

            // Lay thong bao moi nhat cua 1 nguoi cho nhanh
            e.HasIndex(n => new { n.UserId, n.CreatedAt });

            // Chong trung. Postgres cho phep nhieu dong DedupKey = NULL, chi chan khoa that bi lap
            e.HasIndex(n => n.DedupKey).IsUnique();
        });
    }
}

// Do dai toi da cua cac cot (dung chung cho DbContext va bo kiem tra tin nhan)
public static class NotificationLimits
{
    public const int Type = 40;
    public const int Title = 120;
    public const int Body = 500;
    public const int DedupKey = 120;
}
