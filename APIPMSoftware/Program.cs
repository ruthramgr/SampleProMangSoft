using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using APIPMSoftware.Src.Infrastructure.ExternalService;
using APIPMSoftware.Src.Infrastructure.MailerSend;
using APIPMSoftware.Src.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;


var builder = WebApplication.CreateBuilder(args);

// Email strat
// bind MailerSend options from configuration
var msOpts = builder.Configuration.GetSection("MailerSend").Get<MailerSendOptions>()
             ?? throw new InvalidOperationException("MailerSend options missing");

// register options as singleton so we can pass to typed client
builder.Services.AddSingleton(msOpts);

// register HttpClient for MailerSend (you can add retry, timeout, etc.)
builder.Services.AddHttpClient<MailerSendEmailSender>(client =>
{
    client.BaseAddress = new Uri("https://api.mailersend.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    // Authorization header set by MailerSendEmailSender ctor using options
});

// register IEmailSender pointing to the MailerSend implementation
builder.Services.AddScoped<APIPMSoftware.Src.Application.Interface.IEmailSender>(sp =>
    sp.GetRequiredService<MailerSendEmailSender>());

// email end 
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserRegistryRepository, UserRepository>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IPasswordServices, PasswordService>();
// 1️⃣ Register Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "AuthSession:";
});
var app = builder.Build();





app.UseMiddleware<GlobalExceptionMiddleware>();

app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    if (path is not null &&
       (path.StartsWith("/api/auth/") ||
        path.StartsWith("/api/registration") ||
        path.StartsWith("/swagger")))
    {
        await next();
        return;
    }

    var cache = context.RequestServices.GetRequiredService<IDistributedCache>();

    // Access token
    if (context.Request.Cookies.TryGetValue("ACCESS_TOKEN", out var accessToken))
    {
        var userJson = await cache.GetStringAsync($"access:{accessToken}");
        if (userJson != null)
        {
            context.Items["SUser"] = userJson;
            await next();
            return;
        }
    }

    // Refresh token
    if (context.Request.Cookies.TryGetValue("REFRESH_TOKEN", out var refreshToken))
    {
        var userId = await cache.GetStringAsync($"refresh:{refreshToken}");
        if (userId != null)
        {
            var userJson = await cache.GetStringAsync($"user:{userId}");
            if (userJson != null)
            {
                var newAccessToken = Guid.NewGuid().ToString();

                await cache.SetStringAsync(
                    $"access:{newAccessToken}",
                    userJson,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                    });

                context.Response.Cookies.Append(
                    "ACCESS_TOKEN",
                    newAccessToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(1)
                    });

                context.Items["SUser"] = userJson;
                await next();
                return;
            }
        }
    }

    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    return;
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
