// Chuoi tuoi cay kieu Duolingo: ngon lua + so ngay + 7 cham (ngay nao da tuoi thi xanh)
import { View } from "react-native";
import { Icon } from "./Icon";
import { Txt } from "./Txt";
import { useLang } from "../i18n/LanguageContext";
import type { Streak } from "../api/garden";
import { colors, radius } from "../theme";

const DAY_LETTERS = { en: ["S", "M", "T", "W", "T", "F", "S"], vi: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"] };

// "yyyy-MM-dd" -> thu trong tuan (0 = Chu nhat), khong bi lech mui gio
function weekday(date: string) {
  const [y, m, d] = date.split("-").map(Number);
  return new Date(Date.UTC(y, m - 1, d)).getUTCDay();
}

export function StreakWeek({ streak }: { streak: Streak }) {
  const { lang } = useLang();
  const letters = DAY_LETTERS[lang];
  return (
    <View style={{ flexDirection: "row", gap: 8 }}>
      {streak.week.map((d, i) => {
        const today = i === streak.week.length - 1;
        return (
          <View key={d.date} style={{ alignItems: "center", gap: 4 }}>
            <View
              accessibilityLabel={`${d.date}${d.watered ? " ✓" : ""}`}
              style={{
                width: 26,
                height: 26,
                borderRadius: radius.pill,
                alignItems: "center",
                justifyContent: "center",
                backgroundColor: d.watered ? colors.primary : "#FFFFFF",
                borderWidth: today && !d.watered ? 2 : 1,
                borderColor: d.watered ? colors.primary : today ? colors.sun : colors.border,
              }}
            >
              {d.watered ? <Icon name="check" size={14} color="#FFFFFF" strokeWidth={3} /> : null}
            </View>
            <Txt size={10} color={colors.muted}>{letters[weekday(d.date)]}</Txt>
          </View>
        );
      })}
    </View>
  );
}
