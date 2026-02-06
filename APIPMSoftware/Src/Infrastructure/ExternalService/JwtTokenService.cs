using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using Microsoft.Extensions.Options;

namespace APIPMSoftware.Src.Infrastructure.ExternalService
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtsettings;

        public JwtTokenService(IOptions<JwtSettings> settings)
        {
            _jwtsettings = settings.Value;
        }
        public string GenerateToken(int UserId, string UserName, string Email)
        {
            throw new NotImplementedException();
        }
    }
}
