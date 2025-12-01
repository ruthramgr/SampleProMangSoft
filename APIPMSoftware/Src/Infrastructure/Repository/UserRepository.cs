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
            using (SqlCommand cmd= new SqlCommand("sp_CreateUsers", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Email", email);    
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
        public async Task<int?> GetUserIdByEmail(string email)
        {
            using (SqlConnection conn=new SqlConnection(_connectionString))
            using (SqlCommand comm = new SqlCommand("SP_GETUSERIDBYEMAIL", conn))
            {
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.AddWithValue("@EmailId", email);
                await conn.OpenAsync();
                var result = await comm.ExecuteScalarAsync();
                if (result != null && int.TryParse(result.ToString(), out int userid))
                {
                    return userid;
                }
                else
                    return null;
            }
    }
    }
}
