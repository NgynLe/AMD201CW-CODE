// Doi ngon ngu EN/VI (nut trong Settings). Lua chon duoc luu lai, mo app lan sau van giu.
import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { getItem, LANG_KEY, setItem } from "../auth/storage";

const en = {
  // Thanh tab
  navGarden: "Garden",
  navAi: "AI",
  navCommunity: "Community",
  navSettings: "Settings",

  // Dang nhap / dang ky
  welcomeBack: "Welcome back",
  loginSub: "Log in to see which plants need water today.",
  email: "Email",
  password: "Password",
  login: "Log in",
  loggingIn: "Logging in…",
  newHere: "New to TreeO?",
  createAccount: "Create an account",
  registerTitle: "Plant your first seed",
  registerSub: "Create an account to start caring for your desk garden.",
  yourName: "Your name",
  confirmPassword: "Confirm password",
  passwordHint: "At least 8 characters.",
  creatingAccount: "Creating account…",
  haveAccount: "Already have an account?",
  errEmailPassword: "Enter your email and password.",
  errName: "Your name needs at least 2 characters.",
  errEmail: "Enter a valid email address.",
  errPasswordLength: "Password needs at least 8 characters.",
  errPasswordMatch: "The two passwords don't match.",

  // Quen mat khau
  forgotPassword: "Forgot password?",
  forgotTitle: "Reset your password",
  forgotSub: "Enter the email you signed up with. We'll send you a 6-digit code.",
  sendCode: "Send code",
  sending: "Sending…",
  codeSentTo: "We sent a 6-digit code to",
  codeCheckSpam: "It can take a minute. Check your spam folder too.",
  code: "6-digit code",
  resetPassword: "Reset password",
  resetting: "Resetting…",
  resendCode: "Send a new code",
  resendIn: "Send a new code in",
  seconds: "s",
  useOtherEmail: "Use a different email",
  backToLogin: "Back to log in",
  errCode: "Enter the 6-digit code from the email.",

  // Settings
  settingsTitle: "Settings & Profile",
  language: "Language",
  device: "IoT device",
  online: "Online",
  board: "Board",
  ip: "IP address",
  strong: "Strong",
  lastSeen: "Last seen",
  justNow: "Just now",
  sheetsSub: "Moisture & pump logs sync automatically",
  openSheet: "Open sheet",
  exportData: "Export data",
  account: "Account & security",
  password_change: "Change password",
  notifications: "Notifications",
  logout: "Log out",
  roleExpert: "Verified Expert",
  roleAdmin: "Admin",

  // Doi mat khau
  changePasswordTitle: "Change password",
  changePasswordSub: "Other devices will be logged out after you change it.",
  currentPassword: "Current password",
  newPassword: "New password",
  confirmNewPassword: "Confirm new password",
  savePassword: "Save new password",
  saving: "Saving…",
  passwordChanged: "Password changed",
  errCurrentPassword: "Enter your current password.",
  back: "Back",

  // Community
  community: "Community",
  createPost: "Create post",
  allPosts: "All Posts",
  expertGuides: "Expert Guides",
  following: "Following",
  follow: "Follow",
  verifiedExpert: "Verified Expert",
  you: "You",
  like: "Like",
  unlike: "Unlike",
  deletePost: "Delete post",
  emptyAll: "No posts yet. Share the first tip with the community.",
  emptyExperts: "No expert guides yet.",
  emptyFollowing: "You're not following anyone yet. Tap Follow on a post to see that grower's posts here.",
  retry: "Try again",
  justPosted: "Just now",
  minutesAgo: "m ago",
  hoursAgo: "h ago",
  yesterday: "Yesterday",

  // Dang bai
  newPostTitle: "New post",
  newPostPlaceholder: "Share a tip, a question, or how your plant is doing…",
  post: "Post",
  posting: "Posting…",
  cancel: "Cancel",
  errEmptyPost: "Write something before posting.",
  // Thoi tiet, chuoi tuoi cay, thong bao, AI (phan viec cua Than)
  humidity: "Humidity",
  tomorrow: "Tomorrow",
  weatherLoading: "Loading weather…",
  weatherUnavailable: "Weather isn't available right now.",
  heatwaveTomorrow: "Heatwave tomorrow — increase watering",
  heatwaveToday: "Heatwave today — keep the plant out of direct sun",
  cond_clear: "Sunny",
  cond_mostly_clear: "Mostly sunny",
  cond_partly_cloudy: "Partly cloudy",
  cond_overcast: "Overcast",
  cond_fog: "Foggy",
  cond_drizzle: "Drizzle",
  cond_rain: "Rain",
  cond_showers: "Showers",
  cond_snow: "Snow",
  cond_thunderstorm: "Thunderstorm",
  cond_unknown: "Weather",
  streakDays: "-Day Care Streak",
  streakStart: "Water today to start a streak",
  streakAtRisk: "Water today to keep your streak!",
  streakDone: "Watered today",
  streakMilestone: "Streak milestone! Check your notifications.",
  streakFailed: "Couldn't save today's watering to your streak.",
  notificationsTitle: "Notifications",
  notificationsLabel: "Notifications",
  markAllRead: "Mark all as read",
  noNotifications: "No notifications yet. Heatwave alerts and streak milestones will show up here.",
  deleteNotification: "Delete notification",
  notifHeatwaveTitle: "Heatwave tomorrow",
  notifHeatwaveBody: "Up to {temp}°C expected. Water your plants a bit more.",
  notifStreakTitle: "{days}-day care streak!",
  notifStreakBody: "You've watered your plant {days} days in a row. Keep it up!",
  aiModeCare: "Care Plan",
  aiModeFengShui: "Feng Shui",
  aiGreeting: "Hi {name}! I can build a care plan for your plant, or give feng shui tips on which plant to choose and where to place it.",
  aiThinking: "TreeO AI is thinking…",
  aiPlaceholder: "Ask TreeO AI…",
  aiSend: "Send",
  aiDisclaimer: "TreeO AI suggests care plans and feng shui tips — it does not diagnose plant diseases.",
  aiError: "TreeO AI couldn't answer.",
  aiSubtitle: "● Online · care plans & feng shui",
  aiChipCare1: "Weekly plan for my Money Tree",
  aiChipCare2: "Plan for a hot week",
  aiChipCare3: "Care plan while I travel 5 days",
  aiChipFs1: "Best plant for my desk",
  aiChipFs2: "Lucky plants for a new home",
  aiChipFs3: "Where to place a Money Tree",
};

const vi: typeof en = {
  navGarden: "Vườn",
  navAi: "AI",
  navCommunity: "Cộng đồng",
  navSettings: "Cài đặt",

  welcomeBack: "Chào bạn quay lại",
  loginSub: "Đăng nhập để xem hôm nay cây nào cần tưới.",
  email: "Email",
  password: "Mật khẩu",
  login: "Đăng nhập",
  loggingIn: "Đang đăng nhập…",
  newHere: "Mới dùng TreeO?",
  createAccount: "Tạo tài khoản",
  registerTitle: "Gieo mầm đầu tiên",
  registerSub: "Tạo tài khoản để bắt đầu chăm khu vườn nhỏ của bạn.",
  yourName: "Tên của bạn",
  confirmPassword: "Nhập lại mật khẩu",
  passwordHint: "Ít nhất 8 ký tự.",
  creatingAccount: "Đang tạo tài khoản…",
  haveAccount: "Đã có tài khoản?",
  errEmailPassword: "Nhập email và mật khẩu.",
  errName: "Tên cần ít nhất 2 ký tự.",
  errEmail: "Email chưa đúng định dạng.",
  errPasswordLength: "Mật khẩu cần ít nhất 8 ký tự.",
  errPasswordMatch: "Hai mật khẩu chưa khớp nhau.",

  forgotPassword: "Quên mật khẩu?",
  forgotTitle: "Đặt lại mật khẩu",
  forgotSub: "Nhập email bạn đã dùng để đăng ký. TreeO sẽ gửi mã 6 số tới email đó.",
  sendCode: "Gửi mã",
  sending: "Đang gửi…",
  codeSentTo: "Mã 6 số đã được gửi tới",
  codeCheckSpam: "Có thể mất một phút. Nhớ kiểm tra cả thư mục Spam.",
  code: "Mã 6 số",
  resetPassword: "Đặt lại mật khẩu",
  resetting: "Đang đặt lại…",
  resendCode: "Gửi lại mã",
  resendIn: "Gửi lại mã sau",
  seconds: " giây",
  useOtherEmail: "Dùng email khác",
  backToLogin: "Quay lại đăng nhập",
  errCode: "Nhập mã 6 số trong email.",

  settingsTitle: "Cài đặt & Hồ sơ",
  language: "Ngôn ngữ",
  device: "Thiết bị IoT",
  online: "Trực tuyến",
  board: "Bo mạch",
  ip: "Địa chỉ IP",
  strong: "Mạnh",
  lastSeen: "Hoạt động",
  justNow: "Vừa xong",
  sheetsSub: "Nhật ký độ ẩm & bơm tự đồng bộ",
  openSheet: "Mở bảng tính",
  exportData: "Xuất dữ liệu",
  account: "Tài khoản & bảo mật",
  password_change: "Đổi mật khẩu",
  notifications: "Thông báo",
  logout: "Đăng xuất",
  roleExpert: "Chuyên gia",
  roleAdmin: "Quản trị viên",

  changePasswordTitle: "Đổi mật khẩu",
  changePasswordSub: "Sau khi đổi, các thiết bị khác sẽ bị đăng xuất.",
  currentPassword: "Mật khẩu hiện tại",
  newPassword: "Mật khẩu mới",
  confirmNewPassword: "Nhập lại mật khẩu mới",
  savePassword: "Lưu mật khẩu mới",
  saving: "Đang lưu…",
  passwordChanged: "Đã đổi mật khẩu",
  errCurrentPassword: "Nhập mật khẩu hiện tại.",
  back: "Quay lại",

  community: "Cộng đồng",
  createPost: "Đăng bài",
  allPosts: "Tất cả",
  expertGuides: "Chuyên gia",
  following: "Đang theo dõi",
  follow: "Theo dõi",
  verifiedExpert: "Chuyên gia",
  you: "Bạn",
  like: "Thích",
  unlike: "Bỏ thích",
  deletePost: "Xóa bài",
  emptyAll: "Chưa có bài nào. Hãy chia sẻ mẹo đầu tiên với cộng đồng.",
  emptyExperts: "Chưa có bài hướng dẫn của chuyên gia.",
  emptyFollowing: "Bạn chưa theo dõi ai. Bấm Theo dõi trên một bài để xem bài của người đó ở đây.",
  retry: "Thử lại",
  justPosted: "Vừa xong",
  minutesAgo: " phút trước",
  hoursAgo: " giờ trước",
  yesterday: "Hôm qua",

  newPostTitle: "Bài viết mới",
  newPostPlaceholder: "Chia sẻ mẹo, câu hỏi, hoặc tình hình cây của bạn…",
  post: "Đăng",
  posting: "Đang đăng…",
  cancel: "Hủy",
  errEmptyPost: "Hãy viết gì đó trước khi đăng.",
  // Thoi tiet, chuoi tuoi cay, thong bao, AI (phan viec cua Than)
  humidity: "Độ ẩm",
  tomorrow: "Ngày mai",
  weatherLoading: "Đang tải thời tiết…",
  weatherUnavailable: "Hiện chưa xem được thời tiết.",
  heatwaveTomorrow: "Ngày mai nắng nóng — hãy tưới nhiều hơn",
  heatwaveToday: "Hôm nay nắng nóng — tránh để cây dưới nắng gắt",
  cond_clear: "Nắng",
  cond_mostly_clear: "Gần như quang mây",
  cond_partly_cloudy: "Có mây",
  cond_overcast: "U ám",
  cond_fog: "Sương mù",
  cond_drizzle: "Mưa phùn",
  cond_rain: "Mưa",
  cond_showers: "Mưa rào",
  cond_snow: "Tuyết",
  cond_thunderstorm: "Dông",
  cond_unknown: "Thời tiết",
  streakDays: " ngày chăm cây liên tiếp",
  streakStart: "Hãy tưới cây hôm nay để bắt đầu chuỗi",
  streakAtRisk: "Hãy tưới cây hôm nay để giữ chuỗi!",
  streakDone: "Hôm nay đã tưới",
  streakMilestone: "Đạt mốc chuỗi mới! Xem trong thông báo nhé.",
  streakFailed: "Chưa lưu được lần tưới hôm nay vào chuỗi.",
  notificationsTitle: "Thông báo",
  notificationsLabel: "Thông báo",
  markAllRead: "Đánh dấu đã đọc hết",
  noNotifications: "Chưa có thông báo. Cảnh báo nắng nóng và mốc chuỗi tưới cây sẽ hiện ở đây.",
  deleteNotification: "Xóa thông báo",
  notifHeatwaveTitle: "Ngày mai nắng nóng",
  notifHeatwaveBody: "Dự kiến tới {temp}°C. Hãy tưới cây nhiều hơn một chút.",
  notifStreakTitle: "Chuỗi chăm cây {days} ngày!",
  notifStreakBody: "Bạn đã tưới cây {days} ngày liên tiếp. Cố gắng phát huy nhé!",
  aiModeCare: "Lịch chăm sóc",
  aiModeFengShui: "Phong thủy",
  aiGreeting: "Chào {name}! Mình có thể lập lịch chăm sóc cho cây của bạn, hoặc gợi ý phong thủy: nên chọn cây nào và đặt ở đâu.",
  aiThinking: "TreeO AI đang trả lời…",
  aiPlaceholder: "Hỏi TreeO AI…",
  aiSend: "Gửi",
  aiDisclaimer: "TreeO AI gợi ý lịch chăm sóc và phong thủy — không chẩn đoán bệnh của cây.",
  aiError: "TreeO AI chưa trả lời được.",
  aiSubtitle: "● Trực tuyến · lịch chăm sóc & phong thủy",
  aiChipCare1: "Lịch tuần cho cây Kim Tiền",
  aiChipCare2: "Lịch cho tuần nắng nóng",
  aiChipCare3: "Chăm cây khi tôi đi vắng 5 ngày",
  aiChipFs1: "Cây hợp để bàn làm việc",
  aiChipFs2: "Cây may mắn cho nhà mới",
  aiChipFs3: "Nên đặt cây Kim Tiền ở đâu",
};

const strings = { en, vi };

export type Lang = keyof typeof strings;
export type Strings = typeof en;

const LanguageContext = createContext<{ lang: Lang; setLang: (l: Lang) => void; t: Strings } | null>(null);

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Lang>("en");

  // Doc ngon ngu da chon lan truoc
  useEffect(() => {
    getItem(LANG_KEY)
      .then((saved) => {
        if (saved === "en" || saved === "vi") setLangState(saved);
      })
      .catch(() => {});
  }, []);

  function setLang(l: Lang) {
    setLangState(l);
    setItem(LANG_KEY, l).catch(() => {});
  }

  return (
    <LanguageContext.Provider value={{ lang, setLang, t: strings[lang] }}>{children}</LanguageContext.Provider>
  );
}

export function useLang() {
  const ctx = useContext(LanguageContext);
  if (!ctx) throw new Error("useLang must be used inside LanguageProvider");
  return ctx;
}

// "2 gio truoc", "Hom qua"... cho bai dang
export function timeAgo(iso: string, t: Strings, lang: Lang) {
  const diff = Date.now() - new Date(iso).getTime();
  const minutes = Math.floor(diff / 60_000);
  if (minutes < 1) return t.justPosted;
  if (minutes < 60) return `${minutes}${t.minutesAgo}`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}${t.hoursAgo}`;
  if (hours < 48) return t.yesterday;
  return new Date(iso).toLocaleDateString(lang === "vi" ? "vi-VN" : "en-GB", { day: "numeric", month: "short" });
}
