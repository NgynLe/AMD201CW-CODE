using Garden.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Garden.Api.Data;

public class GardenDbContext(DbContextOptions<GardenDbContext> options) : DbContext(options)
{
    public DbSet<WateringDay> WateringDays => Set<WateringDay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WateringDay>(e =>
        {
            e.ToTable("WateringDays");
            // Khoa 2 cot: 1 nguoi chi co 1 dong cho moi ngay (khong the tao trung)
            e.HasKey(w => new { w.UserId, w.Date });
        });
    }
}
