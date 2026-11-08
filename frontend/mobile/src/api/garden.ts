// Goi API cua Garden service (services/garden): chuoi tuoi cay moi ngay
import { GARDEN_URL } from "./config";
import type { AuthFetch } from "./http";

export interface StreakDay {
  date: string;      // yyyy-MM-dd
  watered: boolean;
}

export interface Streak {
  current: number;
  longest: number;
  wateredToday: boolean;
  atRisk: boolean;              // co chuoi nhung hom nay chua tuoi
  totalDays: number;
  lastWateredDate: string | null;
  lastWateredAtUtc: string | null;
  week: StreakDay[];            // 7 ngay, ngay cuoi cung la hom nay
}

export interface WateringResult {
  streak: Streak;
  firstTimeToday: boolean;
  milestoneReached: boolean;
}

export const gardenApi = (authFetch: AuthFetch) => ({
  streak: () => authFetch<Streak>(`${GARDEN_URL}/api/garden/streak`),

  recordWatering: () => authFetch<WateringResult>(`${GARDEN_URL}/api/garden/watering`, { method: "POST" }),
});
