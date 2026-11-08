// Goi API cua AI Assistant service (services/ai-assistant)
import { AI_URL } from "./config";
import type { AuthFetch } from "./http";

export type AiMode = "care" | "fengshui";

export interface ChatTurn {
  role: "user" | "assistant";
  content: string;
}

export const aiApi = (authFetch: AuthFetch) => ({
  // Gui ca lich su hoi dap (service khong luu), tin cuoi la cua nguoi dung
  chat: (mode: AiMode, messages: ChatTurn[]) =>
    authFetch<{ reply: string }>(`${AI_URL}/api/ai/chat`, {
      method: "POST",
      body: JSON.stringify({ mode, messages }),
    }),
});
