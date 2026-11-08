using System.Globalization;
using System.Text.Json;
using Reminder.Api.DTOs;

namespace Reminder.Api.Services;

// Chuyen du lieu JSON tho cua Open-Meteo thanh du lieu cua TreeO.
// Cac ham thuan (khong goi mang) nen viet unit test duoc.
public static class WeatherInterpreter
{
    // Ma thoi tiet WMO (Open-Meteo dung) -> ma ngan gon de app dich va chon icon
    // Bang ma: https://open-meteo.com/en/docs (muc "WMO Weather interpretation codes")
    public static string ConditionFromWmo(int code) => code switch
    {
        0 => "clear",
        1 => "mostly_clear",
        2 => "partly_cloudy",
        3 => "overcast",
        45 or 48 => "fog",
        >= 51 and <= 57 => "drizzle",
        >= 61 and <= 67 => "rain",
        >= 71 and <= 77 => "snow",
        >= 80 and <= 82 => "showers",
        85 or 86 => "snow",
        >= 95 and <= 99 => "thunderstorm",
        _ => "unknown",
    };

    // Nang nong: nhiet do cao nhat trong ngay >= nguong (mac dinh 35 do C)
    public static bool IsHeatwaveDay(double maxTempC, double thresholdC) => maxTempC >= thresholdC;

    public static HeatwaveDto BuildHeatwave(IReadOnlyList<DailyWeatherDto> forecast, double thresholdC)
    {
        var today = forecast.Count > 0 && IsHeatwaveDay(forecast[0].MaxC, thresholdC);
        var tomorrow = forecast.Count > 1 && IsHeatwaveDay(forecast[1].MaxC, thresholdC);
        double? tomorrowMax = forecast.Count > 1 ? forecast[1].MaxC : null;
        return new HeatwaveDto(today, tomorrow, thresholdC, tomorrowMax);
    }

    // Doc JSON tra ve tu https://api.open-meteo.com/v1/forecast (xem OpenMeteoClient de biet cac tham so da xin)
    // Nem InvalidOperationException neu thieu truong can thiet.
    public static WeatherReportDto Parse(
        string json, string locationName, double latitude, double longitude, double heatwaveThresholdC, DateTime nowUtc)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("current", out var cur) || !root.TryGetProperty("daily", out var daily))
            throw new InvalidOperationException("Open-Meteo khong tra ve muc current/daily.");

        var current = new CurrentWeatherDto(
            TemperatureC: Round1(Num(cur, "temperature_2m")),
            FeelsLikeC: Round1(Num(cur, "apparent_temperature")),
            HumidityPercent: (int)Math.Round(Num(cur, "relative_humidity_2m")),
            WindKmh: Round1(Num(cur, "wind_speed_10m")),
            Condition: ConditionFromWmo((int)Math.Round(Num(cur, "weather_code"))),
            IsDay: Num(cur, "is_day") >= 1);

        var dates = Arr(daily, "time");
        var max = Arr(daily, "temperature_2m_max");
        var min = Arr(daily, "temperature_2m_min");
        var rain = Arr(daily, "precipitation_probability_max");
        var codes = Arr(daily, "weather_code");

        var forecast = new List<DailyWeatherDto>();
        for (var i = 0; i < dates.GetArrayLength(); i++)
        {
            // Ngay cuoi cua du bao co the thieu so lieu (null) -> bo qua ngay do
            if (max[i].ValueKind != JsonValueKind.Number || min[i].ValueKind != JsonValueKind.Number) continue;

            forecast.Add(new DailyWeatherDto(
                Date: dates[i].GetString() ?? "",
                MinC: Round1(min[i].GetDouble()),
                MaxC: Round1(max[i].GetDouble()),
                RainChancePercent: rain[i].ValueKind == JsonValueKind.Number ? (int)Math.Round(rain[i].GetDouble()) : 0,
                Condition: codes[i].ValueKind == JsonValueKind.Number ? ConditionFromWmo(codes[i].GetInt32()) : "unknown"));
        }

        if (forecast.Count == 0) throw new InvalidOperationException("Open-Meteo khong co du bao ngay.");

        return new WeatherReportDto(
            locationName, latitude, longitude, nowUtc, current, forecast,
            BuildHeatwave(forecast, heatwaveThresholdC));
    }

    private static double Num(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number)
            throw new InvalidOperationException($"Open-Meteo thieu truong '{name}'.");
        return value.GetDouble();
    }

    private static JsonElement Arr(JsonElement obj, string name)
    {
        if (!obj.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException($"Open-Meteo thieu danh sach '{name}'.");
        return value;
    }

    private static double Round1(double v) => Math.Round(v, 1, MidpointRounding.AwayFromZero);

    // Khoa cache: lam tron toa do 2 so thap phan (~1 km) de nhieu nguoi o gan nhau dung chung 1 ket qua
    public static string CacheKey(double latitude, double longitude) =>
        string.Create(CultureInfo.InvariantCulture, $"weather:{Math.Round(latitude, 2):0.00}:{Math.Round(longitude, 2):0.00}");
}
