namespace APIPMSoftware.Src.Application.Interface
{
    public interface IEmailSender
    {
        
        Task SendAsync(string to,string subject, string vcode);
        
    }

}
