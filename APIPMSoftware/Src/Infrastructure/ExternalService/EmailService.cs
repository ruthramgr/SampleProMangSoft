using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Service;
using Microsoft.Extensions.Options;
using MimeKit;

namespace APIPMSoftware.Src.Infrastructure.ExternalService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _env;

        public EmailService(IOptions<EmailSettings> emailSettings,
            IWebHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env ?? throw new ArgumentNullException(nameof(env), "WebHostEnvironment cannot be null.");
        }
        public async Task<bool> SendVerificationEmailAsync(string toEmail, string verificationCode)
        {

			try
			{
				var email = new MimeMessage();
                email.From.Add(new MailboxAddress("MGR Registration", _emailSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = "🔐 Your Verification Code";

                string basePath = _env.ContentRootPath;

                var templatePath = Path.Combine(basePath, "EmailTemplates", "VerificationEmailTemplate.html");

                if (!File.Exists(templatePath))
                    throw new FileNotFoundException("Verification email template not found.", templatePath);

                string htmlBody = await File.ReadAllTextAsync(templatePath);
                htmlBody = htmlBody.Replace("##VerificationCode##", verificationCode);

                email.Body = new TextPart("html") { Text = htmlBody };

                var smtpClient = new MailKit.Net.Smtp.SmtpClient();
                await smtpClient.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
                return true;
            }
			catch (Exception)
			{

				throw;
			}
        }
    }
}
