using APIPMSoftware.Src.Application.Interface;
using BCrypt.Net;

namespace APIPMSoftware.Src.Infrastructure.ExternalService
{
    public class PasswordService : IPasswordServices
    {
        public string hashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
