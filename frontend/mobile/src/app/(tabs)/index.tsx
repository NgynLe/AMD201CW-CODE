// Man Garden (Home)
// - Dien thoai: 1 cot (mockup "Mobile · Home Dashboard")
// - Man hinh rong: 2 cot + bieu do 24h + hoat dong gan day (mockup "Web · Dashboard")
// Thoi tiet (Reminder service), chuoi tuoi cay (Garden service) va chuong thong bao (Notification service) la DU LIEU THAT.
// So lieu do am/hoat dong van la DU LIEU MAU - se thay bang Sensor service (phan cua Nguyen Le)
import { useEffect, useRef, useState } from "react";
import { Animated, Easing, Platform, Pressable, ScrollView, View } from "react-native";
import { router } from "expo-router";
import { Screen } from "../../components/Screen";
import { Card } from "../../components/Card";
import { Icon } from "../../components/Icon";
import { Txt } from "../../components/Txt";
import { Toggle } from "../../components/Toggle";
import { MoistureRing, moistureLevel } from "../../components/MoistureRing";
import { MoistureChart } from "../../components/MoistureChart";
import { useAuth } from "../../auth/AuthContext";
import { StreakWeek } from "../../components/StreakWeek";
import { useStreak, useUnreadCount, useWeather } from "../../hooks/useGardenData";
import { useIsWide } from "../../hooks/useIsWide";
import { useLang, type Strings } from "../../i18n/LanguageContext";
import type { Streak } from "../../api/garden";
import type { WeatherReport } from "../../api/weather";
import { colors, radius } from "../../theme";

const native = Platform.OS !== "web";

// Du lieu mau cho bieu do 24h: do am giam dan, bom chay luc 15:00
const HISTORY = [66, 64, 62, 60, 58, 56, 54, 51, 49, 46, 43, 40, 37, 34, 31, 68, 65, 61, 58, 55, 52, 49, 45];
const ACTIVITY = [
  { text: "Pump ran 6s — auto stop at 68%", time: "Today, 14:02", dot: colors.water },
  { text: "Daily check-in — streak +1", time: "Today, 08:00", dot: colors.sun },
  { text: "Heatwave alert sent", time: "Yesterday, 21:10", dot: "#E65100" },
  { text: "Synced logs to Google Sheets", time: "Yesterday, 18:30", dot: colors.primary },
];

function greeting() {
  const h = new Date().getHours();
  if (h < 12) return "Good morning";
  if (h < 18) return "Good afternoon";
  return "Good evening";
}

function todayLabel() {
  return new Date().toLocaleDateString("en-GB", { weekday: "long", day: "numeric", month: "long" });
}

// ---------- Cac khoi giao dien dung chung cho ca 2 bo cuc ----------

function StreakFlame() {
  const v = useRef(new Animated.Value(0)).current;
  useEffect(() => {
    const loop = Animated.loop(
      Animated.sequence([
        Animated.timing(v, { toValue: 1, duration: 700, easing: Easing.inOut(Easing.ease), useNativeDriver: native }),
        Animated.timing(v, { toValue: 0, duration: 700, easing: Easing.inOut(Easing.ease), useNativeDriver: native }),
      ]),
    );
    loop.start();
    return () => loop.stop();
  }, [v]);
  const scale = v.interpolate({ inputRange: [0, 1], outputRange: [1, 1.12] });
  const rotate = v.interpolate({ inputRange: [0, 1], outputRange: ["-4deg", "4deg"] });
  return (
    <Animated.View style={{ transform: [{ scale }, { rotate }] }}>
      <Icon name="flame" size={18} />
    </Animated.View>
  );
}

function StreakPill({ streak }: { streak: Streak | null }) {
  const { t } = useLang();
  const has = !!streak && streak.current > 0;
  return (
    <View style={{ gap: 10 }}>
      <View
        style={{
          alignSelf: "flex-start",
          flexDirection: "row",
          alignItems: "center",
          gap: 8,
          paddingVertical: 8,
          paddingHorizontal: 14,
          borderRadius: radius.pill,
          backgroundColor: colors.sunSoft,
          borderWidth: 1,
          borderColor: colors.sunLine,
        }}
      >
        <StreakFlame />
        <Txt weight="semibold" color={colors.warnText}>
          {has ? `${streak!.current}${t.streakDays}` : t.streakStart}
        </Txt>
      </View>
      {streak ? (
        <View style={{ gap: 6 }}>
          <StreakWeek streak={streak} />
          {streak.atRisk ? <Txt size={12} weight="medium" color={colors.warnIcon}>{t.streakAtRisk}</Txt> : null}
          {streak.wateredToday ? <Txt size={12} weight="medium" color={colors.primaryDark}>{t.streakDone}</Txt> : null}
        </View>
      ) : null}
    </View>
  );
}

// Chuong thong bao + huy hieu so thu chua doc. Bam de mo man Thong bao
function BellButton() {
  const { t } = useLang();
  const unread = useUnreadCount();
  return (
    <Pressable
      onPress={() => router.push("/notifications")}
      accessibilityRole="button"
      accessibilityLabel={unread > 0 ? `${t.notificationsLabel} (${unread})` : t.notificationsLabel}
      style={{ width: 44, height: 44, borderRadius: 22, borderWidth: 1, borderColor: colors.border, backgroundColor: "#FFFFFF", alignItems: "center", justifyContent: "center" }}
    >
      <Icon name="bell" size={20} />
      {unread > 0 ? (
        <View style={{ position: "absolute", top: -2, right: -2, minWidth: 18, height: 18, paddingHorizontal: 4, borderRadius: 9, backgroundColor: colors.danger, alignItems: "center", justifyContent: "center" }}>
          <Txt size={10} weight="bold" color="#FFFFFF">{unread > 9 ? "9+" : String(unread)}</Txt>
        </View>
      ) : null}
    </Pressable>
  );
}

function Avatar({ name }: { name?: string }) {
  return (
    <View style={{ width: 44, height: 44, borderRadius: 22, backgroundColor: colors.primary, alignItems: "center", justifyContent: "center" }}>
      <Txt weight="semibold" color="#FFFFFF">{(name ?? "?").charAt(0).toUpperCase()}</Txt>
    </View>
  );
}

function conditionLabel(code: string, t: Strings) {
  const key = `cond_${code}` as keyof Strings;
  return (t[key] as string | undefined) ?? t.cond_unknown;
}

function WeatherCard({ wide, weather, loading, error }: { wide?: boolean; weather: WeatherReport | null; loading: boolean; error: string | null }) {
  const { t } = useLang();

  if (!weather) {
    return (
      <Card style={wide ? { borderRadius: radius.hero, padding: 20 } : null}>
        <View style={{ flexDirection: "row", alignItems: "center", gap: 14 }}>
          <Icon name="sun" size={wide ? 48 : 44} />
          <View style={{ flex: 1, gap: 2 }}>
            <Txt color={colors.muted}>{loading ? t.weatherLoading : t.weatherUnavailable}</Txt>
            {!loading && error ? <Txt size={12} color={colors.muted}>{error}</Txt> : null}
          </View>
        </View>
      </Card>
    );
  }

  const tomorrow = weather.forecast[1];
  return (
    <Card style={wide ? { borderRadius: radius.hero, padding: 20 } : null}>
      <View style={{ flexDirection: "row", alignItems: "center", gap: 14 }}>
        <Icon name="sun" size={wide ? 48 : 44} />
        <View style={{ flex: 1, gap: 2 }}>
          <Txt size={wide ? 30 : 28} weight="bold">{Math.round(weather.current.temperatureC)}°C</Txt>
          <Txt size={13} color={colors.muted}>{conditionLabel(weather.current.condition, t)} · {weather.locationName}</Txt>
        </View>
        <View style={{ alignItems: "flex-end", gap: 2 }}>
          <Txt size={12} color={colors.muted}>{wide && tomorrow ? t.tomorrow : t.humidity}</Txt>
          <Txt size={15} weight="semibold">
            {wide && tomorrow ? `${Math.round(tomorrow.maxC)}°C` : `${weather.current.humidityPercent}%`}
          </Txt>
        </View>
      </View>
      {weather.heatwave.tomorrow || weather.heatwave.today ? (
        <View style={{ flexDirection: "row", gap: 10, alignItems: "center", backgroundColor: colors.warnBg, borderRadius: 12, paddingVertical: 10, paddingHorizontal: 12 }}>
          <Icon name="alert" size={18} color={colors.warnIcon} />
          <Txt size={13} weight="medium" color={colors.warnText} style={{ flex: 1 }}>
            {weather.heatwave.tomorrow ? t.heatwaveTomorrow : t.heatwaveToday}
          </Txt>
        </View>
      ) : null}
    </Card>
  );
}

function StatusPill({ moisture, short }: { moisture: number; short?: boolean }) {
  const level = moistureLevel(moisture);
  const s = {
    dry: { label: "Dry — needs water", fg: colors.dryText, bg: colors.sunSoft },
    optimal: { label: "Optimal", fg: colors.primaryDark, bg: colors.primarySoft },
    wet: { label: short ? "Wet" : "Wet — no watering needed", fg: colors.waterText, bg: colors.waterSoft },
  }[level];
  return (
    <View style={{ backgroundColor: s.bg, paddingVertical: 4, paddingHorizontal: 12, borderRadius: radius.pill, alignSelf: "center" }}>
      <Txt weight="semibold" color={s.fg}>{s.label}</Txt>
    </View>
  );
}

function StatTile({ label, value }: { label: string; value: string }) {
  return (
    <View style={{ flex: 1, minWidth: 96, backgroundColor: colors.bg, borderRadius: 12, paddingVertical: 10, paddingHorizontal: 12, gap: 2 }}>
      <Txt size={12} color={colors.muted}>{label}</Txt>
      <Txt size={15} weight="semibold">{value}</Txt>
    </View>
  );
}

function WaterButton({ watering, onPress, flex }: { watering: boolean; onPress: () => void; flex?: boolean }) {
  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      style={{ flex: flex ? 1 : undefined, height: 48, borderRadius: radius.button, backgroundColor: colors.primary, flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 8 }}
    >
      <Icon name="drop" size={18} color="#FFFFFF" />
      <Txt size={15} weight="semibold" color="#FFFFFF">{watering ? "Watering…" : "Water now"}</Txt>
    </Pressable>
  );
}

function AutoRow({ auto, setAuto, sub }: { auto: boolean; setAuto: (v: boolean) => void; sub: string }) {
  return (
    <View style={{ flexDirection: "row", alignItems: "center", gap: 12 }}>
      <View style={{ flex: 1, gap: 2 }}>
        <Txt size={15} weight="medium">Auto-watering</Txt>
        <Txt size={12} color={colors.muted}>{sub}</Txt>
      </View>
      <Toggle value={auto} onChange={setAuto} label="Auto-watering" />
    </View>
  );
}

function Ripple({ active }: { active: boolean }) {
  const v = useRef(new Animated.Value(0)).current;
  useEffect(() => {
    if (!active) return;
    v.setValue(0);
    const loop = Animated.loop(Animated.timing(v, { toValue: 1, duration: 1200, easing: Easing.out(Easing.ease), useNativeDriver: native }));
    loop.start();
    return () => loop.stop();
  }, [active, v]);
  if (!active) return null;
  return (
    <Animated.View
      pointerEvents="none"
      style={{
        position: "absolute",
        width: 64,
        height: 64,
        borderRadius: 32,
        backgroundColor: colors.primaryLine,
        opacity: v.interpolate({ inputRange: [0, 1], outputRange: [0.55, 0] }),
        transform: [{ scale: v.interpolate({ inputRange: [0, 1], outputRange: [1, 1.9] }) }],
      }}
    />
  );
}

function FloatingPump({ watering, onPress, inset }: { watering: boolean; onPress: () => void; inset: number }) {
  return (
    <View style={{ position: "absolute", right: inset, bottom: inset, width: 64, height: 64, alignItems: "center", justifyContent: "center" }}>
      <Ripple active={watering} />
      <Pressable
        onPress={onPress}
        accessibilityRole="button"
        accessibilityLabel="Pump water"
        style={{ width: 64, height: 64, borderRadius: 32, backgroundColor: colors.primary, alignItems: "center", justifyContent: "center", elevation: 6, shadowColor: colors.primary, shadowOpacity: 0.45, shadowRadius: 10, shadowOffset: { width: 0, height: 6 } }}
      >
        <Icon name="drop" size={28} color="#FFFFFF" fill={colors.waterSoft} strokeWidth={1.6} />
      </Pressable>
    </View>
  );
}

function Toast({ moisture, wide, note }: { moisture: number; wide: boolean; note?: string | null }) {
  return (
    <View
      accessibilityLiveRegion="polite"
      style={[
        { position: "absolute", top: wide ? 24 : 20, backgroundColor: colors.primaryDark, borderRadius: 14, padding: 14, flexDirection: "row", gap: 10, alignItems: "center", elevation: 6 },
        wide ? { right: 32 } : { left: 20, right: 20 },
      ]}
    >
      <Icon name="check" size={18} color={colors.primaryLine} strokeWidth={2.5} />
      <View style={{ flex: 1, gap: 2 }}>
        <Txt weight="medium" color="#FFFFFF">Watering completed — soil is now {moisture}%</Txt>
        {note ? <Txt size={12} color={colors.primaryLine}>{note}</Txt> : null}
      </View>
    </View>
  );
}

// ---------- Man hinh ----------

export default function GardenScreen() {
  const { user } = useAuth();
  const { t } = useLang();
  const wide = useIsWide();
  const { weather, loading: weatherLoading, error: weatherError } = useWeather();
  const { streak, recordWatering } = useStreak();
  const [note, setNote] = useState<string | null>(null);
  const [moisture, setMoisture] = useState(42);
  const [watering, setWatering] = useState(false);
  const [auto, setAuto] = useState(true);
  const [toast, setToast] = useState(false);
  const timers = useRef<ReturnType<typeof setTimeout>[]>([]);

  useEffect(() => () => timers.current.forEach(clearTimeout), []);

  // Gia lap bom nuoc 3 giay (Sensor service la phan cua Nguyen Le). Khi bom xong thi ghi 1 lan tuoi vao chuoi (Garden service)
  function pump() {
    if (watering) return;
    setWatering(true);
    setToast(false);
    timers.current.push(
      setTimeout(() => {
        setWatering(false);
        setMoisture(65);
        setToast(true);
        setNote(null);
        recordWatering().then((res) => {
          if (!res) setNote(t.streakFailed);
          else if (res.milestoneReached) setNote(t.streakMilestone);
        });
        timers.current.push(setTimeout(() => setToast(false), 4000));
      }, 3000),
    );
  }

  if (wide) {
    return (
      <Screen maxWidth={null}>
        <ScrollView contentContainerStyle={{ paddingHorizontal: 32, paddingTop: 16, paddingBottom: 48, gap: 20, maxWidth: 1344, width: "100%" }}>
          {/* Dau trang */}
          <View style={{ flexDirection: "row", flexWrap: "wrap", alignItems: "center", gap: 16 }}>
            <View style={{ flexGrow: 1, flexBasis: 260, gap: 4 }}>
              <Txt color={colors.muted}>{todayLabel()}</Txt>
              <Txt size={28} weight="bold" accessibilityRole="header">Hello, {user?.displayName}!</Txt>
            </View>
            <StreakPill streak={streak} />
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Choose plant"
              style={{ height: 44, borderRadius: 12, borderWidth: 1, borderColor: colors.border, backgroundColor: "#FFFFFF", paddingHorizontal: 14, flexDirection: "row", alignItems: "center", gap: 8 }}
            >
              <Txt>Money Tree · Desk-01</Txt>
              <View style={{ transform: [{ rotate: "90deg" }] }}>
                <Icon name="chevron" size={16} color={colors.muted} />
              </View>
            </Pressable>
            <BellButton />
            <Avatar name={user?.displayName} />
          </View>

          <View style={{ flexDirection: "row", flexWrap: "wrap", gap: 20, alignItems: "flex-start" }}>
            {/* Cot trai: do am + bieu do */}
            <Card style={{ flexGrow: 2, flexBasis: 560, minWidth: 0, borderRadius: radius.hero, padding: 24, gap: 20 }}>
              <View style={{ flexDirection: "row", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 8 }}>
                <Txt size={18} weight="semibold" accessibilityRole="header">Live soil moisture</Txt>
                <Txt size={13} color={colors.muted}>ESP32 Desk-01 · updated 12s ago</Txt>
              </View>
              <View style={{ flexDirection: "row", flexWrap: "wrap", gap: 28, alignItems: "center" }}>
                <MoistureRing moisture={moisture} />
                <View style={{ flexGrow: 1, flexBasis: 240, gap: 14 }}>
                  <View style={{ flexDirection: "row", alignItems: "center", gap: 12, flexWrap: "wrap" }}>
                    <Txt size={56} weight="bold" style={{ lineHeight: 60 }}>{moisture}%</Txt>
                    <StatusPill moisture={moisture} short />
                  </View>
                  <View style={{ flexDirection: "row", gap: 10, flexWrap: "wrap" }}>
                    <StatTile label="Last watered" value="14:02" />
                    <StatTile label="Target range" value="30–70%" />
                    <StatTile label="Auto stop" value="> 60%" />
                  </View>
                </View>
              </View>
              <MoistureChart values={[...HISTORY, moisture]} pumpHour={15} />
            </Card>

            {/* Cot phai */}
            <View style={{ flexGrow: 1, flexBasis: 320, minWidth: 0, gap: 20 }}>
              <WeatherCard wide weather={weather} loading={weatherLoading} error={weatherError} />

              <Card style={{ borderRadius: radius.hero, padding: 20, gap: 14 }}>
                <Txt size={16} weight="semibold" accessibilityRole="header">Quick controls</Txt>
                <WaterButton watering={watering} onPress={pump} />
                <AutoRow auto={auto} setAuto={setAuto} sub="Safety shut-off above 60%" />
                <Pressable
                  accessibilityRole="button"
                  style={{ height: 44, borderRadius: 12, borderWidth: 1, borderColor: colors.primaryLine, backgroundColor: colors.primaryTint, alignItems: "center", justifyContent: "center" }}
                >
                  <Txt weight="semibold" color={colors.primaryDark}>Sync history to Google Sheets</Txt>
                </Pressable>
              </Card>

              <Card style={{ borderRadius: radius.hero, padding: 20 }}>
                <Txt size={16} weight="semibold" accessibilityRole="header">Recent activity</Txt>
                {ACTIVITY.map((a) => (
                  <View key={a.text} style={{ flexDirection: "row", gap: 12, alignItems: "flex-start" }}>
                    <View style={{ width: 10, height: 10, borderRadius: 5, marginTop: 5, backgroundColor: a.dot }} />
                    <View style={{ gap: 2, flex: 1 }}>
                      <Txt weight="medium">{a.text}</Txt>
                      <Txt size={12} color={colors.muted}>{a.time}</Txt>
                    </View>
                  </View>
                ))}
              </Card>
            </View>
          </View>
        </ScrollView>

        {toast && <Toast moisture={moisture} wide note={note} />}
        <FloatingPump watering={watering} onPress={pump} inset={32} />
      </Screen>
    );
  }

  // ----- Bo cuc dien thoai -----
  return (
    <Screen>
      <ScrollView contentContainerStyle={{ padding: 20, paddingBottom: 110, gap: 16 }}>
        <View style={{ flexDirection: "row", justifyContent: "space-between", alignItems: "center" }}>
          <View style={{ gap: 2, flex: 1 }}>
            <Txt color={colors.muted}>{greeting()}</Txt>
            <Txt size={24} weight="bold" numberOfLines={1}>Hello, {user?.displayName}!</Txt>
          </View>
          <View style={{ flexDirection: "row", gap: 10, alignItems: "center" }}>
            <BellButton />
            <Avatar name={user?.displayName} />
          </View>
        </View>

        <StreakPill streak={streak} />
        <WeatherCard weather={weather} loading={weatherLoading} error={weatherError} />

        <Card style={{ borderRadius: radius.hero, padding: 20, alignItems: "center" }}>
          <View style={{ alignSelf: "stretch", flexDirection: "row", justifyContent: "space-between", alignItems: "center" }}>
            <Txt size={16} weight="semibold">Money Tree</Txt>
            <Txt size={12} color={colors.muted}>ESP32 · Desk-01</Txt>
          </View>
          <MoistureRing moisture={moisture} />
          <View style={{ alignItems: "center", gap: 4 }}>
            <Txt size={40} weight="bold" style={{ lineHeight: 44 }}>{moisture}%</Txt>
            <StatusPill moisture={moisture} />
            <Txt size={12} color={colors.muted} style={{ marginTop: 4 }}>Soil moisture · updated 12s ago · tap for 24h history</Txt>
          </View>
        </Card>

        <Card style={{ gap: 14 }}>
          <Txt size={16} weight="semibold">Quick controls</Txt>
          <View style={{ flexDirection: "row", gap: 10 }}>
            <WaterButton watering={watering} onPress={pump} flex />
            <Pressable
              accessibilityRole="button"
              style={{ flex: 1, height: 48, borderRadius: radius.button, borderWidth: 1, borderColor: colors.primaryLine, backgroundColor: colors.primaryTint, flexDirection: "row", alignItems: "center", justifyContent: "center", gap: 8 }}
            >
              <Icon name="sheet" size={18} color={colors.primaryDark} />
              <Txt size={15} weight="semibold" color={colors.primaryDark}>Sync to Sheets</Txt>
            </Pressable>
          </View>
          <AutoRow auto={auto} setAuto={setAuto} sub="Pump stops automatically above 60%" />
        </Card>
      </ScrollView>

      {toast && <Toast moisture={moisture} wide={false} note={note} />}
      <FloatingPump watering={watering} onPress={pump} inset={20} />
    </Screen>
  );
}
