using System.Text;
using System.Threading.RateLimiting;
using AiAssistant.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ===== JWT: chi KIEM TRA token do Identity cap, phai cung Issuer/Audience/Key voi Identity =====
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key phai dai it nhat 32 ky tu (xem appsettings.Development.json).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

// ===== CORS: danh sach trang web duoc goi API (chi can khi chay app tren trinh duyet) =====
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy("web", policy => policy
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ===== Gioi han so cau hoi: moi nguoi toi da N cau / phut (tranh ton tien API AI) =====
var perMinute = builder.Configuration.GetValue("Ai:RequestsPerMinute", 10);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("chat", httpContext =>
    {
        var user = httpContext.User.FindFirst("sub")?.Value
                   ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                   ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(user, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = perMinute,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });
});

// ===== AI: chon nha cung cap bang "Ai:Provider" = "gemini" (mac dinh, co goi mien phi) hoac "anthropic" =====
// API key dat bang dotnet user-secrets: "Gemini:ApiKey" hoac "Anthropic:ApiKey"
var provider = (builder.Configuration["Ai:Provider"] ?? "gemini").Trim().ToLowerInvariant();
if (provider == "anthropic")
{
    var anthropic = builder.Configuration.GetSection("Anthropic").Get<AnthropicSettings>() ?? new AnthropicSettings();
    builder.Services.AddSingleton(anthropic);
    builder.Services.AddHttpClient<IChatClient, AnthropicChatClient>(client =>
    {
        client.BaseAddress = new Uri(anthropic.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(60);
    });
}
else
{
    var gemini = builder.Configuration.GetSection("Gemini").Get<GeminiSettings>() ?? new GeminiSettings();
    builder.Services.AddSingleton(gemini);
    builder.Services.AddHttpClient<IChatClient, GeminiChatClient>(client =>
    {
        client.BaseAddress = new Uri(gemini.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(60);
    });
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// Kiem tra nhanh service con song khong (khong can dang nhap)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "ai-assistant" }));

app.Run();
