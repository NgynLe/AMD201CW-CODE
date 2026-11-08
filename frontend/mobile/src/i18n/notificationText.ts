// Dich noi dung thong bao theo ngon ngu dang chon.
// Loai da biet (heatwave, streak) -> dung ban dich trong LanguageContext + tham so server gui kem (Data).
// Loai la (vd "pump" cua may bom) -> hien Title/Body tieng Anh do ben gui dat san.
import type { AppNotification } from "../api/notifications";
import type { Strings } from "./LanguageContext";

export function notificationText(n: AppNotification, t: Strings): { title: string; body: string } {
  if (n.type === "heatwave" && n.data?.maxTemp) {
    return { title: t.notifHeatwaveTitle, body: t.notifHeatwaveBody.replace("{temp}", n.data.maxTemp) };
  }
  if (n.type === "streak" && n.data?.days) {
    return {
      title: t.notifStreakTitle.replace("{days}", n.data.days),
      body: t.notifStreakBody.replace("{days}", n.data.days),
    };
  }
  return { title: n.title, body: n.body };
}
