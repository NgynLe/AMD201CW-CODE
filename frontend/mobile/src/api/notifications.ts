// Goi API cua Notification service (services/notification)
import { NOTIFICATION_URL } from "./config";
import type { AuthFetch } from "./http";

export interface AppNotification {
  id: string;
  type: string;                        // heatwave, streak, pump... (loai la thi hien title/body do server gui)
  title: string;
  body: string;
  data: Record<string, string> | null; // tham so de dich, vd { maxTemp: "36" }
  createdAt: string;
  isRead: boolean;
}

export interface NotificationList {
  unreadCount: number;
  items: AppNotification[];
}

export const notificationsApi = (authFetch: AuthFetch) => ({
  list: (take = 50) => authFetch<NotificationList>(`${NOTIFICATION_URL}/api/notifications?take=${take}`),

  markRead: (id: string) => authFetch<void>(`${NOTIFICATION_URL}/api/notifications/${id}/read`, { method: "POST" }),

  markAllRead: () => authFetch<void>(`${NOTIFICATION_URL}/api/notifications/read-all`, { method: "POST" }),

  remove: (id: string) => authFetch<void>(`${NOTIFICATION_URL}/api/notifications/${id}`, { method: "DELETE" }),
});
