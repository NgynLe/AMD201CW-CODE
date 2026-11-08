using AiAssistant.Api.DTOs;
using AiAssistant.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AiAssistant.Api.Controllers;

// Can dang nhap (gui kem token). Gioi han so cau hoi de tranh ton tien API (xem Program.cs: policy "chat").
[ApiController]
[Route("api/ai")]
[Authorize]
[EnableRateLimiting("chat")]
public class ChatController(IChatClient chat, ILogger<ChatController> logger) : ControllerBase
{
    // POST /api/ai/chat   { "mode": "care", "messages": [ { "role": "user", "content": "..." } ] }
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat(ChatRequest req, CancellationToken ct)
    {
        if (CurrentUser.Id(User) is null) return Unauthorized();

        var error = ChatRules.Validate(req);
        if (error is not null) return BadRequest(new ErrorResponse(error));

        var messages = ChatRules.MergeSameRole(ChatRules.Trim(req.Messages));
        var system = ChatRules.SystemPrompt(req.Mode, CurrentUser.Name(User));

        try
        {
            var reply = await chat.CompleteAsync(system, messages, ct);
            return Ok(new ChatResponse(reply));
        }
        catch (ChatProviderException ex)
        {
            logger.LogWarning("Chat that bai ({Status}): {Message}", ex.StatusCode, ex.Message);
            return StatusCode(ex.StatusCode, new ErrorResponse(ex.Message));
        }
    }
}
