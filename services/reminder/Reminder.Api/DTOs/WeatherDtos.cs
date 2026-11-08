namespace Reminder.Api.DTOs;

// Thoi tiet hien tai
//   Condition: ma ngan gon (clear, partly_cloudy, rain...) de app tu dich va chon icon
public record CurrentWeatherDto(
    double TemperatureC,
    double FeelsLikeC,
    int HumidityPercent,
    double WindKmh,
    string Condition,
    bool IsDay);

// Du bao 1 ngay
public record DailyWeatherDto(
    string Date,
    double MinC,
    double MaxC,
    int RainChancePercent,
    string Condition);

// Canh bao nang nong (ngay >= ThresholdC do C theo cau hinh Weather:HeatwaveThresholdC)
public record HeatwaveDto(
    bool Today,
    bool Tomorrow,
    double ThresholdC,
    double? TomorrowMaxC);

// Du lieu tra ve cho app
public record WeatherReportDto(
    string LocationName,
    double Latitude,
    double Longitude,
    DateTime FetchedAtUtc,
    CurrentWeatherDto Current,
    List<DailyWeatherDto> Forecast,   // ngay 0 = hom nay, ngay 1 = ngay mai...
    HeatwaveDto Heatwave);

// App doc loi tu truong "message" (giong Identity va Forum)
public record ErrorResponse(string Message);
