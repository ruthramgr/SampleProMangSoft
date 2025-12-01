namespace APIPMSoftware.Src.Application.Interface
{
    public interface IUserRegistryRepository
    {
        Task<int>CreateUserAsync(string username,string email,string companyName);

        Task<int?>GetUserIdByEmail(string email);
    }
}
