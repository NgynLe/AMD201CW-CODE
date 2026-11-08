namespace Garden.Api.Services;

// Tinh chuoi tuoi cay tu danh sach cac ngay da tuoi.
// Ham thuan (khong dung database, khong dung gio he thong) nen viet unit test duoc.
public record StreakSummary(
    int Current,
    int Longest,
    bool WateredToday,
    bool AtRisk,
    int TotalDays,
    DateOnly? LastWateredDate);

public static class StreakCalculator
{
    // Cac moc se gui thong bao chuc mung
    private static readonly int[] Milestones = [3, 7, 14, 30, 60, 100, 200, 365];

    public static bool IsMilestone(int streakDays) => Array.IndexOf(Milestones, streakDays) >= 0;

    // "Hom nay" theo gio dia phuong = gio UTC cong them utcOffsetHours (Viet Nam = 7)
    public static DateOnly TodayAt(DateTime utcNow, int utcOffsetHours) =>
        DateOnly.FromDateTime(utcNow.AddHours(utcOffsetHours));

    public static StreakSummary Compute(IEnumerable<DateOnly> wateredDays, DateOnly today)
    {
        // Bo ngay trung va ngay trong tuong lai (may chinh sai gio)
        var days = wateredDays.Where(d => d <= today).Distinct().OrderBy(d => d).ToList();
        var set = days.ToHashSet();
        var wateredToday = set.Contains(today);

        // Chuoi hien tai: dem lui tu hom nay (neu hom nay da tuoi) hoac tu hom qua.
        // Chua tuoi hom nay thi chuoi VAN CON cho den het ngay, giong Duolingo.
        var cursor = wateredToday ? today : today.AddDays(-1);
        var current = 0;
        while (set.Contains(cursor))
        {
            current++;
            cursor = cursor.AddDays(-1);
        }

        // Chuoi dai nhat: duyet cac ngay theo thu tu, ngay nao noi tiep ngay truoc thi cong don
        var longest = 0;
        var run = 0;
        DateOnly? previous = null;
        foreach (var day in days)
        {
            run = previous is DateOnly p && day == p.AddDays(1) ? run + 1 : 1;
            longest = Math.Max(longest, run);
            previous = day;
        }

        return new StreakSummary(
            Current: current,
            Longest: longest,
            WateredToday: wateredToday,
            AtRisk: !wateredToday && current > 0,
            TotalDays: days.Count,
            LastWateredDate: days.Count > 0 ? days[^1] : (DateOnly?)null);
    }
}
