// Dia chi cac service backend.
// - Chay tren trinh duyet cung may: dung mac dinh (localhost)
// - Chay tren dien thoai that: tao file .env o thu muc mobile:
//     EXPO_PUBLIC_API_URL=http://<IP-may-tinh>:5101
//     EXPO_PUBLIC_FORUM_URL=http://<IP-may-tinh>:5107
//     EXPO_PUBLIC_GARDEN_URL=http://<IP-may-tinh>:5103         (chuoi tuoi cay)
//     EXPO_PUBLIC_REMINDER_URL=http://<IP-may-tinh>:5104       (thoi tiet)
//     EXPO_PUBLIC_NOTIFICATION_URL=http://<IP-may-tinh>:5105   (thong bao)
//     EXPO_PUBLIC_AI_URL=http://<IP-may-tinh>:5106             (TreeO AI)
//   (dien thoai va may tinh phai cung mang Wi-Fi)
const trim = (url: string) => url.replace(/\/$/, "");

export const API_URL = trim(process.env.EXPO_PUBLIC_API_URL ?? "http://localhost:5101");       // Identity
export const FORUM_URL = trim(process.env.EXPO_PUBLIC_FORUM_URL ?? "http://localhost:5107");   // Forum
export const GARDEN_URL = trim(process.env.EXPO_PUBLIC_GARDEN_URL ?? "http://localhost:5103");                   // Garden (chuoi tuoi cay)
export const REMINDER_URL = trim(process.env.EXPO_PUBLIC_REMINDER_URL ?? "http://localhost:5104");               // Reminder (thoi tiet)
export const NOTIFICATION_URL = trim(process.env.EXPO_PUBLIC_NOTIFICATION_URL ?? "http://localhost:5105");       // Notification
export const AI_URL = trim(process.env.EXPO_PUBLIC_AI_URL ?? "http://localhost:5106");                           // AI Assistant
