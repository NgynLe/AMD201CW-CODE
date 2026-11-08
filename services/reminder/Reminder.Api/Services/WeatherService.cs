using Microsoft.Extensions.Caching.Memory;
using Reminder.Api.DTOs;

namespace Reminder.Api.Services;

// Lay thoi tiet (co nho tam trong bo nho) de khong goi Open-Meteo moi lan nguoi dung mo app.
public class WeatherService(OpenMeteoClient client, IMemoryCache cache, IConfiguration config)
{
    public async Task<WeatherReportDto> GetAsync(
        double latitude, double longitude, string locationName, CancellationToken ct)
    {
        var key = WeatherInterpreter.CacheKey(latitude, longitude);

        if (!cache.TryGetValue(key, out WeatherReportDto? report) || report is null)
        {
            var threshold = config.GetValue("Weather:HeatwaveThresholdC", 35.0);
            var json = await client.GetForecastJsonAsync(latitude, longitude, ct);
            report = WeatherInterpreter.Parse(json, locationName, latitude, longitude, threshold, DateTime.UtcNow);

            var minutes = config.GetValue("Weather:CacheMinutes", 10);
            cache.Set(key, report, TimeSpan.FromMinutes(minutes));
        }

        // Ten dia diem theo nguoi goi, khong theo ban luu tam
        return report with { LocationName = locationName };
    }
}
