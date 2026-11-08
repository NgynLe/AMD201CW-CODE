using System.Net;
using System.Text;
using System.Text.Json;
using AiAssistant.Api.DTOs;

namespace AiAssistant.Api.Services;

// Loi khi goi nha cung cap AI. Message la cau tieng Anh an toan de hien thang cho nguoi dung.
public class ChatProviderException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public interface IChatClient
{
    Task<string> CompleteAsync(string systemPrompt, IReadOnlyList<ChatMessageDto> messages, CancellationToken ct);
}

public class AnthropicSettings
{
    public string ApiKey { get; set; } = "";                    // dat bang dotnet user-secrets, KHONG de trong code
    public string Model { get; set; } = "claude-sonnet-5-5";
    public int MaxTokens { get; set; } = 700;
    public string BaseUrl { get; set; } = "https://api.anthropic.com/";
}

// Goi Messages API cua Anthropic bang HttpClient thuong (khong can them package).
// Tai lieu: https://docs.claude.com/en/api/messages
public class AnthropicChatClient(HttpClient http, AnthropicSettings settings, ILogger<AnthropicChatClient> logger) : IChatClient
{
    public async Task<string> CompleteAsync(string systemPrompt, IReadOnlyList<ChatMessageDto> messages, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new ChatProviderException("The AI service isn't set up yet (missing API key).", 503);

        var payload = new
        {
            model = settings.Model,
            max_tokens = settings.MaxTokens,
            system = systemPrompt,
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-api-key", settings.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning("Khong goi duoc AI: {Message}", ex.Message);
            throw new ChatProviderException("Can't reach the AI service right now. Please try again.", 502);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AI tra loi loi {Status}: {Body}", (int)response.StatusCode, Truncate(body, 300));
                var message = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "The AI service rejected our API key.",
                    HttpStatusCode.TooManyRequests => "The AI service is busy. Please try again in a moment.",
                    _ => "The AI service had a problem. Please try again.",
                };
                throw new ChatProviderException(message, 502);
            }

            return ParseReply(body);
        }
    }

    // Lay phan chu trong cau tra loi: { "content": [ { "type": "text", "text": "..." } ] }
    public static string ParseReply(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var sb = new StringBuilder();
            foreach (var block in doc.RootElement.GetProperty("content").EnumerateArray())
            {
                if (block.TryGetProperty("type", out var t) && t.GetString() == "text" &&
                    block.TryGetProperty("text", out var text))
                    sb.Append(text.GetString());
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
