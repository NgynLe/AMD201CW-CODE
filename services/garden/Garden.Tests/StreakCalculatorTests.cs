using Garden.Api.Services;

namespace Garden.Tests;

// Test logic tinh chuoi tuoi cay (khong can database, khong can mang). Chay: dotnet test
public class StreakCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 7);

    private static DateOnly DaysAgo(int n) => Today.AddDays(-n);

    [Fact]
    public void NoWatering_StreakIsZero_AndNotAtRisk()
    {
        var s = StreakCalculator.Compute([], Today);

        Assert.Equal(0, s.Current);
        Assert.Equal(0, s.Longest);
        Assert.False(s.WateredToday);
        Assert.False(s.AtRisk);
        Assert.Equal(0, s.TotalDays);
        Assert.Null(s.LastWateredDate);
    }

    [Fact]
    public void WateredOnlyToday_StreakIsOne()
    {
        var s = StreakCalculator.Compute([Today], Today);

        Assert.Equal(1, s.Current);
        Assert.Equal(1, s.Longest);
        Assert.True(s.WateredToday);
        Assert.False(s.AtRisk);
        Assert.Equal(Today, s.LastWateredDate);
    }

    [Fact]
    public void NotWateredYetToday_StreakSurvives_ButIsAtRisk()
    {
        // Tuoi hom qua va hom kia, hom nay chua tuoi: chuoi van la 2 nhung sap mat
        var s = StreakCalculator.Compute([DaysAgo(2), DaysAgo(1)], Today);

        Assert.Equal(2, s.Current);
        Assert.False(s.WateredToday);
        Assert.True(s.AtRisk);
    }

    [Fact]
    public void MissedAFullDay_StreakResetsToZero()
    {
        // Lan tuoi cuoi la 2 ngay truoc: da bo lo tron 1 ngay (hom qua)
        var s = StreakCalculator.Compute([DaysAgo(3), DaysAgo(2)], Today);

        Assert.Equal(0, s.Current);
        Assert.Equal(2, s.Longest);       // ky luc cu van duoc giu
        Assert.False(s.AtRisk);
    }

    [Fact]
    public void GapBreaksCurrentStreak()
    {
        // Hom nay, hom qua co tuoi; 2 ngay truoc bo lo; 3 ngay truoc co tuoi
        var s = StreakCalculator.Compute([Today, DaysAgo(1), DaysAgo(3)], Today);

        Assert.Equal(2, s.Current);
        Assert.Equal(2, s.Longest);
        Assert.Equal(3, s.TotalDays);
    }

    [Fact]
    public void LongestStreak_CanBeInThePast()
    {
        // 5 ngay lien tiep cach day 10..14 ngay, roi chi tuoi hom nay
        var past = Enumerable.Range(10, 5).Select(DaysAgo);
        var s = StreakCalculator.Compute(past.Append(Today), Today);

        Assert.Equal(1, s.Current);
        Assert.Equal(5, s.Longest);
        Assert.Equal(6, s.TotalDays);
    }

    [Fact]
    public void DuplicateDays_AreCountedOnce()
    {
        var s = StreakCalculator.Compute([Today, Today, DaysAgo(1), DaysAgo(1)], Today);

        Assert.Equal(2, s.Current);
        Assert.Equal(2, s.TotalDays);
    }

    [Fact]
    public void FutureDays_AreIgnored()
    {
        var s = StreakCalculator.Compute([Today.AddDays(1), Today.AddDays(2)], Today);

        Assert.Equal(0, s.Current);
        Assert.Equal(0, s.Longest);
        Assert.Equal(0, s.TotalDays);
    }

    [Fact]
    public void UnorderedInput_GivesSameResult()
    {
        var s = StreakCalculator.Compute([DaysAgo(2), Today, DaysAgo(1)], Today);

        Assert.Equal(3, s.Current);
        Assert.Equal(3, s.Longest);
    }

    [Fact]
    public void TodayAt_UsesVietnamOffset_AcrossMidnight()
    {
        // 18:00 UTC = 01:00 hom sau theo gio Viet Nam
        var late = new DateTime(2026, 11, 7, 18, 0, 0, DateTimeKind.Utc);
        Assert.Equal(new DateOnly(2026, 11, 8), StreakCalculator.TodayAt(late, 7));

        // 16:59 UTC = 23:59 cung ngay theo gio Viet Nam
        var early = new DateTime(2026, 11, 7, 16, 59, 0, DateTimeKind.Utc);
        Assert.Equal(new DateOnly(2026, 11, 7), StreakCalculator.TodayAt(early, 7));
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(7, true)]
    [InlineData(30, true)]
    [InlineData(365, true)]
    [InlineData(1, false)]
    [InlineData(8, false)]
    [InlineData(0, false)]
    public void Milestones(int days, bool expected)
    {
        Assert.Equal(expected, StreakCalculator.IsMilestone(days));
    }
}
