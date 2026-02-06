namespace APIPMSoftware.Src.Application.Interface
{
    public interface IUserRegistryRepository
    {
        Task<int>CreateUserAsync(string username,string email,string companyName);

        Task<int?>GetUserIdByEmail(string email);

        Task UpdatePasswordAsync(int userId,string passwordHash);

        Task<(int? UserId,string? PasswordHass,bool? IsAdmin)> GetUserCredentialsByEmail(string email);
    }
}
