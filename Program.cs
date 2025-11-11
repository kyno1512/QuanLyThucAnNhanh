using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtProvider>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<MonService>();
builder.Services.AddScoped<GioHangService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PromotionService>();
builder.Services.AddScoped<InventoryAdminService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // thêm dòng này nếu có cookie/token
    });
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic validation để tự xử lý
        options.SuppressModelStateInvalidFilter = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Đặt đúng thứ tự middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();   // ⚠️ Quan trọng
app.UseStaticFiles();        // Cho phép phục vụ ảnh trong wwwroot
app.UseCors("AllowReact");   // ⚠️ CORS phải nằm trước Authentication

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
