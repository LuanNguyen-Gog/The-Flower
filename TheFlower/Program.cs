using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Models;
using Repository.Repositories.Interfaces;
using Repository.Repositories.Implementations;
using Service.Services.Interfaces;
using Service.Services.Implementations;
using TheFlower.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ── Kestrel Configuration - Support both HTTP and HTTPS ────────────────────────
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(5000, listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1;
        });

        serverOptions.ListenAnyIP(5001, listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
            listenOptions.UseHttps();
        });
    });
}

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SalesAppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Unit of Work & Repository DI ──────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Individual repositories (optional, can use UnitOfWork instead)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IStoreLocationRepository, StoreLocationRepository>();


// ── Service DI ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStoreLocationService, StoreLocationService>();

// ── Chat Service ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IChatService, ChatService>();
// ── HttpClient for External APIs ──────────────────────────────────────────────
builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };

    // Hỗ trợ JWT qua query string cho SignalR WebSocket
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hub/chat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger with JWT support ──────────────────────────────────────────────────
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Flower API",
        Version = "v1",
        Description = "RESTful API for The Flower Android mobile app"
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token (không cần tiền tố 'Bearer')",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// ── CORS (cho Android client) ─────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Database migration + admin seed from appsettings ──────────────────────────
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesAppDBContext>();
    await dbContext.Database.MigrateAsync();

    var adminSection = app.Configuration.GetSection("AdminAccount");
    var adminUsername = adminSection["Username"];
    var adminEmail = adminSection["Email"];
    var adminPasswordHash = adminSection["PasswordHash"];

    if (!string.IsNullOrWhiteSpace(adminUsername)
        && !string.IsNullOrWhiteSpace(adminEmail)
        && !string.IsNullOrWhiteSpace(adminPasswordHash))
    {
        var adminUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == adminUsername || u.Email == adminEmail);

        if (adminUser is null)
        {
            dbContext.Users.Add(new User
            {
                Username = adminUsername,
                Email = adminEmail,
                PasswordHash = adminPasswordHash,
                Role = adminSection["Role"] ?? "Admin",
                PhoneNumber = adminSection["PhoneNumber"],
                Address = adminSection["Address"],
                Status = adminSection["Status"] ?? "Active",
                CreatedAt = DateTime.UtcNow.AddHours(7)
            });
        }
        else
        {
            adminUser.PasswordHash = adminPasswordHash;
            adminUser.Role = adminSection["Role"] ?? "Admin";
            adminUser.PhoneNumber = adminSection["PhoneNumber"];
            adminUser.Address = adminSection["Address"];
            adminUser.Status = adminSection["Status"] ?? "Active";
        }

        await dbContext.SaveChangesAsync();
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// ── HTTPS Redirection - Only in Production ────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hub/chat");
app.MapHub<NotificationHub>("/hub/notifications");

// ── Start background services ─────────────────────────────────────────────────
var chatService = app.Services.GetRequiredService<IChatService>();
await chatService.StartAsync();

app.Run();
