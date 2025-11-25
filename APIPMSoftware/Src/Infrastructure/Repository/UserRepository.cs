using APIPMSoftware.Src.Application.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace APIPMSoftware.Src.Infrastructure.Repository
{
    public class UserRepository : IUserRegistryRepository
    {

        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<int> CreateUserAsync(string username, string email, string companyName)
        {
            using (SqlConnection conn=new SqlConnection(_connectionString))
            using (SqlCommand cmd= new SqlCommand("",conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@CompanyName", companyName);
                SqlParameter userIdParameter = new SqlParameter("@UserId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(userIdParameter);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                var userid=(int)userIdParameter.Value;
                if(userid==-1)
                {
                    throw new Exception("Email already registered");
                }
                return userid;
            }            
        }
    }
}
