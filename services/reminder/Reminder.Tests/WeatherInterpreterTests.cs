using Reminder.Api.DTOs;
using Reminder.Api.Services;

namespace Reminder.Tests;

// Test phan doc du lieu thoi tiet (khong can Internet). Chay: dotnet test
public class WeatherInterpreterTests
{
    // Mau JSON rut gon theo dung dinh dang Open-Meteo tra ve
    private const string SampleJson = """
    {
      "latitude": 10.8, "longitude": 106.6,
      "current": {
        "time": "2026-11-07T14:00", "interval": 900,
        "temperature_2m": 33.4, "relative_humidity_2m": 62, "apparent_temperature": 38.26,
        "is_day": 1, "weather_code": 2, "wind_speed_10m": 11.04
      },
      "daily": {
        "time": ["2026-11-07", "2026-11-08", "2026-11-09"],
        "weather_code": [2, 0, 95],
        "temperature_2m_max": [34.1, 36.0, 31.2],
        "temperature_2m_min": [26.0, 27.5, 25.1],
        "precipitation_probability_max": [10, 0, 80]
      }
    }
    """;

    private static readonly DateTime Now = new(2026, 11, 7, 7, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Parse_ReadsCurrentWeather()
    {
        var r = WeatherInterpreter.Parse(SampleJson, "Ho Chi Minh City", 10.82, 106.63, 35, Now);

        Assert.Equal("Ho Chi Minh City", r.LocationName);
        Assert.Equal(33.4, r.Current.TemperatureC);
        Assert.Equal(38.3, r.Current.FeelsLikeC);      // lam tron 1 so le
        Assert.Equal(62, r.Current.HumidityPercent);
        Assert.Equal(11.0, r.Current.WindKmh);
        Assert.Equal("partly_cloudy", r.Current.Condition);
        Assert.True(r.Current.IsDay);
        Assert.Equal(Now, r.FetchedAtUtc);
    }

    [Fact]
    public void Parse_ReadsThreeDayForecast()
    {
        var r = WeatherInterpreter.Parse(SampleJson, "x", 0, 0, 35, Now);

        Assert.Equal(3, r.Forecast.Count);
        Assert.Equal("2026-11-08", r.Forecast[1].Date);
        Assert.Equal(36.0, r.Forecast[1].MaxC);
        Assert.Equal(27.5, r.Forecast[1].MinC);
        Assert.Equal("clear", r.Forecast[1].Condition);
        Assert.Equal(80, r.Forecast[2].RainChancePercent);
        Assert.Equal("thunderstorm", r.Forecast[2].Condition);
    }

    [Fact]
    public void Parse_FlagsHeatwaveTomorrow_WhenMaxReachesThreshold()
    {
        var r = WeatherInterpreter.Parse(SampleJson, "x", 0, 0, 35, Now);

        Assert.False(r.Heatwave.Today);       // hom nay toi da 34.1 < 35
        Assert.True(r.Heatwave.Tomorrow);     // ngay mai 36.0 >= 35
        Assert.Equal(36.0, r.Heatwave.TomorrowMaxC);
        Assert.Equal(35, r.Heatwave.ThresholdC);
    }

    [Fact]
    public void Parse_NoHeatwave_WhenThresholdIsHigher()
    {
        var r = WeatherInterpreter.Parse(SampleJson, "x", 0, 0, 37, Now);

        Assert.False(r.Heatwave.Today);
        Assert.False(r.Heatwave.Tomorrow);
    }

    [Fact]
    public void Parse_SkipsDaysWithMissingTemperature()
    {
        const string json = """
        {
          "current": { "temperature_2m": 30, "relative_humidity_2m": 70, "apparent_temperature": 33,
                       "is_day": 0, "weather_code": 0, "wind_speed_10m": 5 },
          "daily": {
            "time": ["2026-11-07", "2026-11-08"],
            "weather_code": [1, null],
            "temperature_2m_max": [32, null],
            "temperature_2m_min": [25, null],
            "precipitation_probability_max": [null, null]
          }
        }
        """;

        var r = WeatherInterpreter.Parse(json, "x", 0, 0, 35, Now);

        Assert.Single(r.Forecast);
        Assert.Equal(0, r.Forecast[0].RainChancePercent);   // null -> 0
        Assert.False(r.Current.IsDay);
        Assert.False(r.Heatwave.Tomorrow);
        Assert.Null(r.Heatwave.TomorrowMaxC);
    }

    [Fact]
    public void Parse_Throws_WhenFieldsAreMissing()
    {
        Assert.Throws<InvalidOperationException>(() =>
            WeatherInterpreter.Parse("""{ "foo": 1 }""", "x", 0, 0, 35, Now));
    }

    [Theory]
    [InlineData(0, "clear")]
    [InlineData(1, "mostly_clear")]
    [InlineData(2, "partly_cloudy")]
    [InlineData(3, "overcast")]
    [InlineData(45, "fog")]
    [InlineData(53, "drizzle")]
    [InlineData(63, "rain")]
    [InlineData(75, "snow")]
    [InlineData(81, "showers")]
    [InlineData(95, "thunderstorm")]
    [InlineData(999, "unknown")]
    public void ConditionFromWmo_MapsCodes(int code, string expected)
    {
        Assert.Equal(expected, WeatherInterpreter.ConditionFromWmo(code));
    }

    [Theory]
    [InlineData(34.9, 35, false)]
    [InlineData(35.0, 35, true)]
    [InlineData(40.0, 35, true)]
    public void IsHeatwaveDay_UsesThreshold(double max, double threshold, bool expected)
    {
        Assert.Equal(expected, WeatherInterpreter.IsHeatwaveDay(max, threshold));
    }

    [Fact]
    public void BuildHeatwave_HandlesShortForecast()
    {
        var one = new List<DailyWeatherDto> { new("2026-11-07", 25, 40, 0, "clear") };

        var h = WeatherInterpreter.BuildHeatwave(one, 35);

        Assert.True(h.Today);
        Assert.False(h.Tomorrow);
        Assert.Null(h.TomorrowMaxC);
    }

    [Fact]
    public void CacheKey_RoundsToTwoDecimals_AndIgnoresCulture()
    {
        Assert.Equal("weather:10.82:106.63", WeatherInterpreter.CacheKey(10.8231, 106.6297));
        Assert.Equal(WeatherInterpreter.CacheKey(10.8201, 106.6301), WeatherInterpreter.CacheKey(10.8249, 106.6349));
    }
}
