namespace APIPMSoftware.Src.Application.Interface
{
    public interface IJwtTokenService
    {
        string GenerateToken(int UserId,string UserName,string Email);
    }
}
