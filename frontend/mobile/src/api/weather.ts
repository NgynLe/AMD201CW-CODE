// Goi API cua Reminder service (services/reminder): thoi tiet + canh bao nang nong
import { REMINDER_URL } from "./config";
import type { AuthFetch } from "./http";

export interface CurrentWeather {
  temperatureC: number;
  feelsLikeC: number;
  humidityPercent: number;
  windKmh: number;
  condition: string;   // clear, partly_cloudy, cloudy, fog, drizzle, rain, snow, thunderstorm...
  isDay: boolean;
}

export interface DailyWeather {
  date: string;        // yyyy-MM-dd
  minC: number;
  maxC: number;
  rainChancePercent: number;
  condition: string;
}

export interface Heatwave {
  today: boolean;
  tomorrow: boolean;
  thresholdC: number;
  tomorrowMaxC: number | null;
}

export interface WeatherReport {
  locationName: string;
  latitude: number;
  longitude: number;
  fetchedAtUtc: string;
  current: CurrentWeather;
  forecast: DailyWeather[];   // [0] = hom nay, [1] = ngay mai
  heatwave: Heatwave;
}

export const weatherApi = (authFetch: AuthFetch) => ({
  // Khong gui toa do -> server dung mac dinh TP. Ho Chi Minh
  get: () => authFetch<WeatherReport>(`${REMINDER_URL}/api/weather`),
});
