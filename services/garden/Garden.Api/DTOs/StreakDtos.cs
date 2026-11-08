namespace Garden.Api.DTOs;

// 1 cham trong hang 7 ngay (kieu Duolingo): ngay nao da tuoi thi Watered = true
public record StreakDayDto(string Date, bool Watered);

// Du lieu chuoi tuoi cay tra ve cho app
//   Current         so ngay lien tiep dang giu (neu hom nay chua tuoi nhung hom qua co tuoi thi chuoi van con)
//   Longest         chuoi dai nhat tung dat
//   WateredToday    hom nay da tuoi chua
//   AtRisk          chuoi dang co nhung hom nay chua tuoi -> het ngay se mat chuoi
//   TotalDays       tong so ngay da tuoi
//   LastWateredDate ngay tuoi gan nhat (yyyy-MM-dd) hoac null
//   Week            7 ngay gan nhat, ngay cuoi cung la hom nay
public record StreakDto(
    int Current,
    int Longest,
    bool WateredToday,
    bool AtRisk,
    int TotalDays,
    string? LastWateredDate,
    DateTime? LastWateredAtUtc,
    List<StreakDayDto> Week);

// Ket qua sau khi bam "Tuoi cay"
//   FirstTimeToday    lan tuoi dau tien trong ngay (ngay moi vao chuoi)
//   MilestoneReached  vua dat moc (3, 7, 14, 30... ngay) -> da gui thong bao qua RabbitMQ
public record WateringResultDto(StreakDto Streak, bool FirstTimeToday, bool MilestoneReached);

// App doc loi tu truong "message" (giong Identity va Forum)
public record ErrorResponse(string Message);
