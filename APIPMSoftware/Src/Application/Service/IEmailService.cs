namespace APIPMSoftware.Src.Application.Service
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail,string verificationCode);
    }
}
