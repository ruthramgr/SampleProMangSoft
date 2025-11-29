using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using APIPMSoftware.Src.Infrastructure;
using Microsoft.AspNetCore.Routing.Template;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace APIPMSoftware.Src.Infrastructure.MailerSend
{
    public class MailerSendEmailSender : IEmailSender
    {
        private readonly HttpClient _http;
        private readonly MailerSendOptions _opts;
        private readonly IWebHostEnvironment _env;

        public MailerSendEmailSender(HttpClient http, MailerSendOptions opts, IWebHostEnvironment env)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _opts = opts ?? throw new ArgumentNullException(nameof(opts));
            // Ensure API key header for client (also set in Program.cs)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _opts.ApiKey);
            _env = env;
        }

        public async Task SendAsync(string to,string subject,string verificationCode)
        {
            CancellationToken ct = CancellationToken.None;
            string basePath = _env.ContentRootPath;           
            var templatePath = Path.Combine(basePath, "src\\EmailTemplates", "VerificationEmailTemplate.html");

            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Verification email template not found.", templatePath);
            string htmlBody = await File.ReadAllTextAsync(templatePath);
            htmlBody = htmlBody.Replace("##VerificationCode##", verificationCode);
            // Build MailerSend payload based on their Email API:
            // https://developers.mailersend.com/api/email.html#send-an-email
            var payload = new
            {
                from = new { email = _opts.FromEmail, name = _opts.FromName },
                to = new[] { new { email = to } },
                subject = subject,
                html = htmlBody
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // POST to MailerSend
            using var resp = await _http.PostAsync("https://api.mailersend.com/v1/email", content, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                // throw or handle according to your app policy
                throw new InvalidOperationException($"MailerSend returned {(int)resp.StatusCode}: {body}");
            }
        }

       
    }
}
