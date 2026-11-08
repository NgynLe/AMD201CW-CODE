using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiAssistant.Api.DTOs;

namespace AiAssistant.Api.Services;

public class GeminiSettings
{
    public string ApiKey { get; set; } = "";                    // dat bang dotnet user-secrets, KHONG de trong code
    public string Model { get; set; } = "gemini-3.6-flash";     // doi trong appsettings neu Google ngung model nay
    public int MaxTokens { get; set; } = 2048;                  // gom ca token "suy nghi" cua model, nen de rong
    public string ThinkingLevel { get; set; } = "";             // de trong = mac dinh cua model; "minimal" hoac "low" = tra loi nhanh hon (chi cho model Gemini 3)
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";
}

// Goi Gemini API (Google AI Studio, co goi mien phi) bang HttpClient thuong.
// Tai lieu: https://ai.google.dev/api/generate-content
// Cung giao dien IChatClient voi AnthropicChatClient nen doi nha cung cap chi can sua cau hinh Ai:Provider.
public class GeminiChatClient(HttpClient http, GeminiSettings settings, ILogger<GeminiChatClient> logger) : IChatClient
{
    public async Task<string> CompleteAsync(string systemPrompt, IReadOnlyList<ChatMessageDto> messages, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new ChatProviderException("The AI service isn't set up yet (missing API key).", 503);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{settings.Model}:generateContent")
        {
            Content = new StringContent(BuildPayload(systemPrompt, messages, settings.MaxTokens, settings.ThinkingLevel), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-goog-api-key", settings.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning("Khong goi duoc Gemini: {Message}", ex.Message);
            throw new ChatProviderException("Can't reach the AI service right now. Please try again.", 502);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Gemini tra loi loi {Status}: {Body}", (int)response.StatusCode, Truncate(body, 400));
                var message = response.StatusCode switch
                {
                    // Gemini bao key sai bang 400 (API_KEY_INVALID) hoac 403
                    HttpStatusCode.BadRequest when body.Contains("API_KEY_INVALID") => "The AI service rejected our API key.",
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "The AI service rejected our API key.",
                    HttpStatusCode.NotFound => $"The AI model '{settings.Model}' wasn't found. Check Gemini:Model in appsettings.",
                    HttpStatusCode.TooManyRequests => "The AI service is busy or the free quota is used up. Please try again in a moment.",
                    _ => "The AI service had a problem. Please try again.",
                };
                throw new ChatProviderException(message, 502);
            }

            return ParseReply(body);
        }
    }

    // Gemini dung role "user" va "model" (khong phai "assistant"), noi dung nam trong parts[].text
    public static string BuildPayload(string systemPrompt, IReadOnlyList<ChatMessageDto> messages, int maxTokens, string? thinkingLevel = null)
    {
        var contents = new JsonArray();
        foreach (var m in messages)
        {
            contents.Add(new JsonObject
            {
                ["role"] = m.Role == "assistant" ? "model" : "user",
                ["parts"] = new JsonArray(new JsonObject { ["text"] = m.Content }),
            });
        }

        var generationConfig = new JsonObject { ["maxOutputTokens"] = maxTokens };
        if (!string.IsNullOrWhiteSpace(thinkingLevel))
            generationConfig["thinkingConfig"] = new JsonObject { ["thinkingLevel"] = thinkingLevel.Trim() };

        var payload = new JsonObject
        {
            ["systemInstruction"] = new JsonObject { ["parts"] = new JsonArray(new JsonObject { ["text"] = systemPrompt }) },
            ["contents"] = contents,
            ["generationConfig"] = generationConfig,
        };
        return payload.ToJsonString();
    }

    // Lay chu trong cau tra loi: { "candidates": [ { "content": { "parts": [ { "text": "..." } ] } } ] }
    public static string ParseReply(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                // Cau hoi bi chan boi bo loc an toan cua Google
                if (root.TryGetProperty("promptFeedback", out var fb) && fb.TryGetProperty("blockReason", out _))
                    throw new ChatProviderException("The AI can't answer that question. Try asking something about your plants.", 502);
                throw new ChatProviderException("The AI returned an empty answer. Please try again.", 502);
            }

            var sb = new StringBuilder();
            var content = candidates[0].GetProperty("content");
            if (content.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text)) sb.Append(text.GetString());
                }
            }

            var reply = sb.ToString().Trim();
            if (reply.Length == 0) throw new ChatProviderException("The AI returned an empty answer. Please try again.", 502);
            return reply;
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            throw new ChatProviderException("The AI returned an unexpected answer. Please try again.", 502);
        }
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
}
