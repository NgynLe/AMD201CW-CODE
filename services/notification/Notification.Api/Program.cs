using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notification.Api.Data;
using Notification.Api.Messaging;
using Notification.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== Database (Postgres trong Docker): notification_db =====
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<NotificationIngestor>();

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

// ===== RabbitMQ: chay nen, nghe tin nhan thong bao =====
var rabbit = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
builder.Services.AddSingleton(rabbit);
builder.Services.AddHostedService<NotificationConsumer>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ===== Tu tao/cap nhat bang khi khoi dong (can da chay: dotnet ef migrations add InitialCreate) =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Kiem tra nhanh service con song khong (khong can dang nhap)
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "notification" }));

app.Run();
