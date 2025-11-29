namespace APIPMSoftware.Src.Application.DTO
{
    public class MailerSendOptions
    {
        public string ApiKey { get; set; } = null!;
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
    }
}
