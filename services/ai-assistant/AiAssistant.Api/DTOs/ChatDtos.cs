using System.ComponentModel.DataAnnotations;

namespace AiAssistant.Api.DTOs;

// Mot tin nhan trong cuoc tro chuyen. Role chi nhan "user" hoac "assistant".
public record ChatMessageDto([Required] string Role, [Required] string Content);

// Du lieu app gui len.
//   Mode      "care" (ke hoach cham soc) hoac "fengshui" (phong thuy)
//   Messages  lich su hoi dap, tin cuoi cung phai la cua "user" (app gui ca lich su vi service khong luu)
public record ChatRequest([Required] string Mode, [Required] List<ChatMessageDto> Messages);

public record ChatResponse(string Reply);

// App doc loi tu truong "message" (giong Identity va Forum)
public record ErrorResponse(string Message);
