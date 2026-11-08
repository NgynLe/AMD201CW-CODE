// Man TreeO AI: 2 che do Care Plan / Feng Shui, chat
// Cau tra loi that tu AI Assistant service (services/ai-assistant); service goi mo hinh AI bang API key dat o phia server
import { useRef, useState } from "react";
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, TextInput, View, type ScrollViewInstance } from "react-native";
import { Screen } from "../../components/Screen";
import { Icon } from "../../components/Icon";
import { Txt } from "../../components/Txt";
import { useAuth } from "../../auth/AuthContext";
import { aiApi, type AiMode, type ChatTurn } from "../../api/ai";
import { useLang } from "../../i18n/LanguageContext";
import { keyboardBehavior } from "../../hooks/useKeyboardVisible";
import { cardShadow, colors, fonts, radius } from "../../theme";

// kind "greeting": loi chao, hien theo ngon ngu dang chon (khong gui len AI)
type Message = { id: number; role: "ai" | "user"; text: string; kind?: "greeting" | "error" };

const MODE_KEYS: AiMode[] = ["care", "fengshui"];

export default function AiScreen() {
  const { user, authFetch } = useAuth();
  const { t } = useLang();
  const [mode, setMode] = useState(0);
  const [draft, setDraft] = useState("");
  const [sending, setSending] = useState(false);
  const [messages, setMessages] = useState<Message[]>([{ id: 1, role: "ai", text: "", kind: "greeting" }]);
  const scroll = useRef<ScrollViewInstance>(null);

  const modeLabels = [t.aiModeCare, t.aiModeFengShui];
  const chips = [
    [t.aiChipCare1, t.aiChipCare2, t.aiChipCare3],
    [t.aiChipFs1, t.aiChipFs2, t.aiChipFs3],
  ];

  // Doi che do -> bat dau cuoc tro chuyen moi (tranh tron lich su Care Plan voi Feng Shui)
  function changeMode(i: number) {
    if (i === mode || sending) return;
    setMode(i);
    setMessages([{ id: Date.now(), role: "ai", text: "", kind: "greeting" }]);
  }

  async function send() {
    const q = draft.trim();
    if (!q || sending) return;

    // Lich su gui len AI: bo loi chao va tin loi, doi "ai" -> "assistant"
    const history: ChatTurn[] = [
      ...messages
        .filter((m) => !m.kind)
        .map((m): ChatTurn => ({ role: m.role === "ai" ? "assistant" : "user", content: m.text })),
      { role: "user", content: q },
    ];

    setMessages((m) => [...m, { id: Date.now(), role: "user", text: q }]);
    setDraft("");
    setSending(true);
    try {
      const res = await aiApi(authFetch).chat(MODE_KEYS[mode], history);
      setMessages((m) => [...m, { id: Date.now() + 1, role: "ai", text: res.reply }]);
    } catch (e) {
      setMessages((m) => [...m, { id: Date.now() + 1, role: "ai", kind: "error", text: `${t.aiError} ${(e as Error).message}` }]);
    } finally {
      setSending(false);
    }
  }

  return (
    <Screen>
      {/* Ban phim mo -> day o nhap len tren ban phim */}
      <KeyboardAvoidingView style={{ flex: 1 }} behavior={keyboardBehavior}>
      {/* Dau trang + chon che do */}
      <View style={{ paddingHorizontal: 20, paddingTop: 20, paddingBottom: 12, gap: 14, backgroundColor: "#FFFFFF", borderBottomWidth: 1, borderBottomColor: colors.divider }}>
        <View style={{ flexDirection: "row", alignItems: "center", gap: 12 }}>
          <View style={{ width: 44, height: 44, borderRadius: 14, backgroundColor: colors.primarySoft, alignItems: "center", justifyContent: "center" }}>
            <Icon name="sparkle" size={24} color={colors.primary} />
          </View>
          <View style={{ flex: 1, gap: 2 }}>
            <Txt size={20} weight="bold">TreeO AI</Txt>
            <Txt size={12} weight="medium" color={colors.primary}>{t.aiSubtitle}</Txt>
          </View>
        </View>
        <View accessibilityRole="tablist" style={{ flexDirection: "row", gap: 4, backgroundColor: "#EEF5EE", borderRadius: 12, padding: 4 }}>
          {modeLabels.map((label, i) => {
            const on = mode === i;
            return (
              <Pressable
                key={label}
                accessibilityRole="tab"
                accessibilityState={{ selected: on }}
                onPress={() => changeMode(i)}
                style={[
                  { flex: 1, height: 36, borderRadius: 9, alignItems: "center", justifyContent: "center", backgroundColor: on ? "#FFFFFF" : "transparent" },
                  on ? cardShadow : null,
                ]}
              >
                <Txt size={13} weight="semibold" color={on ? colors.primaryDark : colors.muted}>{label}</Txt>
              </Pressable>
            );
          })}
        </View>
      </View>

      {/* Tin nhan */}
      <ScrollView
        ref={scroll}
        style={{ flex: 1 }}
        contentContainerStyle={{ padding: 20, gap: 12 }}
        onContentSizeChange={() => scroll.current?.scrollToEnd({ animated: true })}
      >
        {messages.map((m) =>
          m.role === "user" ? (
            <View key={m.id} style={{ alignSelf: "flex-end", maxWidth: "78%", backgroundColor: "#C8E6C9", paddingVertical: 10, paddingHorizontal: 14, borderRadius: 18, borderBottomRightRadius: 4 }}>
              <Txt color="#1B3A1E" style={{ lineHeight: 20 }}>{m.text}</Txt>
            </View>
          ) : (
            <View key={m.id} style={[{ alignSelf: "flex-start", maxWidth: "86%", backgroundColor: "#FFFFFF", borderWidth: 1, borderColor: colors.divider, paddingVertical: 12, paddingHorizontal: 14, borderRadius: 18, borderBottomLeftRadius: 4, gap: 6 }, cardShadow]}>
              <Txt style={{ lineHeight: 21 }} color={m.kind === "error" ? colors.danger : colors.text}>
                {m.kind === "greeting" ? t.aiGreeting.replace("{name}", user?.displayName ?? "") : m.text}
              </Txt>
            </View>
          ),
        )}
        {sending ? <Txt size={12} color={colors.muted}>{t.aiThinking}</Txt> : null}
      </ScrollView>

      {/* Goi y nhanh */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={{ flexGrow: 0 }} contentContainerStyle={{ paddingHorizontal: 20, paddingTop: 8, gap: 8 }}>
        {chips[mode].map((c) => (
          <Pressable
            key={c}
            onPress={() => setDraft(c)}
            accessibilityRole="button"
            style={{ height: 34, paddingHorizontal: 12, borderRadius: radius.pill, borderWidth: 1, borderColor: colors.primaryLine, backgroundColor: "#FFFFFF", justifyContent: "center" }}
          >
            <Txt size={12} weight="medium" color={colors.primaryDark}>{c}</Txt>
          </Pressable>
        ))}
      </ScrollView>

      {/* O nhap */}
      <View style={{ flexDirection: "row", gap: 8, alignItems: "center", paddingHorizontal: 16, paddingTop: 10 }}>
        <TextInput
          value={draft}
          onChangeText={setDraft}
          onSubmitEditing={send}
          placeholder={t.aiPlaceholder}
          placeholderTextColor="#8A968B"
          accessibilityLabel={t.aiPlaceholder}
          returnKeyType="send"
          style={[
            { flex: 1, height: 46, borderRadius: 23, borderWidth: 1, borderColor: colors.border, backgroundColor: "#FFFFFF", paddingHorizontal: 16, fontFamily: fonts.regular, fontSize: 14, color: colors.text },
            Platform.OS === "web" ? ({ outlineColor: colors.primary } as object) : null,
          ]}
        />
        <Pressable
          onPress={send}
          accessibilityRole="button"
          accessibilityLabel={t.aiSend}
          disabled={sending}
          style={{ width: 46, height: 46, borderRadius: 23, backgroundColor: sending ? colors.track : colors.primary, alignItems: "center", justifyContent: "center" }}
        >
          <Icon name="send" size={20} color="#FFFFFF" />
        </Pressable>
      </View>
      <Txt size={11} color={colors.muted} align="center" style={{ paddingHorizontal: 20, paddingTop: 8, paddingBottom: 12 }}>
        {t.aiDisclaimer}
      </Txt>
      </KeyboardAvoidingView>
    </Screen>
  );
}
