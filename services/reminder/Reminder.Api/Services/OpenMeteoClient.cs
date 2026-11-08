using System.Globalization;

namespace Reminder.Api.Services;

// Goi Open-Meteo (mien phi, khong can API key): https://open-meteo.com
// HttpClient duoc cau hinh (dia chi goc, thoi gian cho toi da) trong Program.cs.
public class OpenMeteoClient(HttpClient http)
{
    // Tra ve JSON tho. WeatherInterpreter.Parse se doc JSON nay.
    public async Task<string> GetForecastJsonAsync(double latitude, double longitude, CancellationToken ct)
    {
        var lat = latitude.ToString("0.####", CultureInfo.InvariantCulture);
        var lon = longitude.ToString("0.####", CultureInfo.InvariantCulture);

        // current: thoi tiet luc nay | daily: du bao theo ngay | timezone=auto: ngay tinh theo gio tai dia diem do
        // Don vi mac dinh cua Open-Meteo: do C, km/h
        var url =
            $"v1/forecast?latitude={lat}&longitude={lon}" +
            "&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,weather_code,wind_speed_10m" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
            "&timezone=auto&forecast_days=3";

        using var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }
}
