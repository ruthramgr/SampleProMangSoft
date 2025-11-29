using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using APIPMSoftware.Src.Infrastructure.Repository;
using APIPMSoftware.Src.Infrastructure.MailerSend;
using Microsoft.AspNetCore.Identity.UI.Services;


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

var app = builder.Build();

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
