using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reminder.Api.Messaging;
using Reminder.Api.Services;

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

// ===== Thoi tiet: goi Open-Meteo, nho tam trong bo nho =====
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<OpenMeteoClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Weather:BaseUrl"] ?? "https://api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<WeatherService>();

// ===== RabbitMQ: gui thong bao nang nong =====
var rabbit = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
builder.Services.AddSingleton(rabbit);
builder.Services.AddSingleton<INotificationPublisher, RabbitMqNotificationPublisher>();

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
app.MapControllers();

// Kiem tra nhanh service con song khong (khong can dang nhap)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "reminder" }));

app.Run();
