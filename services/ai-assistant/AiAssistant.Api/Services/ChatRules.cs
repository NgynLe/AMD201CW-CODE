using AiAssistant.Api.DTOs;

namespace AiAssistant.Api.Services;

// Quy tac kiem tra yeu cau chat va cach dat "vai tro" cho AI.
// Ham thuan (khong goi mang) nen viet unit test duoc.
public static class ChatRules
{
    public const int MaxMessages = 20;          // chi lay 20 tin gan nhat de tiet kiem token
    public const int MaxMessageLength = 1000;   // moi tin toi da 1000 ky tu

    public static readonly string[] Modes = ["care", "fengshui"];

    // Tra ve loi (tieng Anh cho app hien) hoac null neu hop le
    public static string? Validate(ChatRequest? req)
    {
        if (req is null) return "Request is empty.";
        if (!Modes.Contains(req.Mode)) return "Mode must be 'care' or 'fengshui'.";
        if (req.Messages is null || req.Messages.Count == 0) return "Send at least one message.";

        foreach (var m in req.Messages)
        {
            if (m.Role is not ("user" or "assistant")) return "Message role must be 'user' or 'assistant'.";
            if (string.IsNullOrWhiteSpace(m.Content)) return "Messages can't be empty.";
            if (m.Content.Length > MaxMessageLength) return $"Each message can be at most {MaxMessageLength} characters.";
        }

        if (req.Messages[^1].Role != "user") return "The last message must come from the user.";
        return null;
    }

    // Giu toi da MaxMessages tin gan nhat, va dam bao tin dau tien la cua "user" (API cua AI yeu cau)
    public static List<ChatMessageDto> Trim(IReadOnlyList<ChatMessageDto> messages)
    {
        var tail = messages.Count > MaxMessages ? messages.Skip(messages.Count - MaxMessages).ToList() : messages.ToList();
        while (tail.Count > 0 && tail[0].Role != "user") tail.RemoveAt(0);
        return tail;
    }

    // Noi cac tin cung vai tro lien tiep thanh 1 (API cua AI yeu cau vai tro phai xen ke user/assistant)
    public static List<ChatMessageDto> MergeSameRole(IEnumerable<ChatMessageDto> messages)
    {
        var result = new List<ChatMessageDto>();
        foreach (var m in messages)
        {
            if (result.Count > 0 && result[^1].Role == m.Role)
                result[^1] = result[^1] with { Content = result[^1].Content + "\n\n" + m.Content.Trim() };
            else
                result.Add(m with { Content = m.Content.Trim() });
        }
        return result;
    }

    public static string SystemPrompt(string mode, string? userName)
    {
        var who = string.IsNullOrWhiteSpace(userName) ? "the user" : userName.Trim();
        var common =
            "You are TreeO AI, the assistant inside the TreeO smart plant care app (a small desk garden with a soil " +
            "moisture sensor and an automatic water pump). Keep answers short and practical, in plain text without " +
            "markdown headings. Answer in the same language the user writes in (English or Vietnamese). " +
            "You do NOT diagnose plant diseases or pests; if asked, say you can't diagnose and suggest a local " +
            "nursery or an expert. Stay on the topic of plants and plant care. " +
            $"The user's name is {who}.";

        return mode == "fengshui"
            ? common + " Mode: Feng Shui. Suggest which plant suits a space or goal and where to place it, " +
              "and say clearly that feng shui is a cultural tradition, not science."
            : common + " Mode: Care Plan. Build simple weekly care plans (watering checks, light, cleaning, " +
              "what to watch for). For watering, refer to soil moisture percent (the app's sensor target is about 30-70%) " +
              "instead of fixed schedules.";
    }
}
