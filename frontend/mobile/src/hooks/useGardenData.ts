// Cac hook tai du lieu that cho man Garden: thoi tiet (Reminder), chuoi tuoi cay (Garden), so thong bao chua doc (Notification).
// Moi hook tu bat loi: that bai thi tra ve error, man hinh van chay binh thuong (khong lam sap app).
import { useCallback, useState } from "react";
import { useFocusEffect } from "expo-router";
import { useAuth } from "../auth/AuthContext";
import { gardenApi, type Streak, type WateringResult } from "../api/garden";
import { notificationsApi } from "../api/notifications";
import { weatherApi, type WeatherReport } from "../api/weather";

export function useWeather() {
  const { authFetch } = useAuth();
  const [weather, setWeather] = useState<WeatherReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      setWeather(await weatherApi(authFetch).get());
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setLoading(false);
    }
  }, [authFetch]);

  // Moi lan quay lai man Garden thi tai lai (server cache 10 phut nen khong ton them)
  useFocusEffect(
    useCallback(() => {
      load();
    }, [load]),
  );

  return { weather, loading, error, reload: load };
}

export function useStreak() {
  const { authFetch } = useAuth();
  const [streak, setStreak] = useState<Streak | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setStreak(await gardenApi(authFetch).streak());
      setError(null);
    } catch (e) {
      setError((e as Error).message);
    }
  }, [authFetch]);

  useFocusEffect(
    useCallback(() => {
      load();
    }, [load]),
  );

  // Goi sau khi tuoi cay xong. Tra ve null neu loi (khong nem loi ra ngoai)
  const recordWatering = useCallback(async (): Promise<WateringResult | null> => {
    try {
      const result = await gardenApi(authFetch).recordWatering();
      setStreak(result.streak);
      setError(null);
      return result;
    } catch (e) {
      setError((e as Error).message);
      return null;
    }
  }, [authFetch]);

  return { streak, error, recordWatering, reload: load };
}

export function useUnreadCount() {
  const { authFetch } = useAuth();
  const [unread, setUnread] = useState(0);

  useFocusEffect(
    useCallback(() => {
      let alive = true;
      notificationsApi(authFetch)
        .list(1)
        .then((r) => alive && setUnread(r.unreadCount))
        .catch(() => alive && setUnread(0));
      return () => {
        alive = false;
      };
    }, [authFetch]),
  );

  return unread;
}
