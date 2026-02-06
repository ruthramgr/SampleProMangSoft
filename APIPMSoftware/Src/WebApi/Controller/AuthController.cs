using APIPMSoftware.Src.Application.Common;
using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace APIPMSoftware.Src.WebApi.Controller
{
    [ApiController]
    [Route("api/auth")]
  
    public class AuthController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly IUserRegistryRepository _userRepository;

        public AuthController(IDistributedCache cache,IUserRegistryRepository user)
        {            
            _cache = cache;
            _userRepository = user;
        }
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            var (userId, passwordHash, isAdmin) = await _userRepository.GetUserCredentialsByEmail(request.Email);
            if (userId == null || passwordHash == null)
                return Unauthorized(ResponseApiDynamic<string>.Fail("User not found"));

            bool isPasswordValid =BCrypt.Net.BCrypt.Verify(request.Password,passwordHash);
            if (!isPasswordValid)
                return Unauthorized(ResponseApiDynamic<string>.Fail("Invalid password"));

            var user = new User
            {
                UserId = userId,
                passwordHash = passwordHash
            };
           var accessToken = Guid.NewGuid().ToString(); // Replace with actual token generation logic
            var refreshToken = Guid.NewGuid().ToString();
            var data = JsonSerializer.Serialize(user);
            
            await _cache.SetStringAsync(
                $"access:{accessToken}",
                JsonSerializer.Serialize(user),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                });

            await _cache.SetStringAsync(
                $"refresh:{refreshToken}",
                 user.UserId.ToString(),               
               new DistributedCacheEntryOptions
               {
                   AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
               });

            await _cache.SetStringAsync(                     
               $"user:{user.UserId}",
               JsonSerializer.Serialize(user),
              new DistributedCacheEntryOptions
              {
                  AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
              });

            Response.Cookies.Append("ACCESS_TOKEN", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(1)
            });

            Response.Cookies.Append("REFRESH_TOKEN", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            return Ok(ResponseApiDynamic<object>.Success(user, "Login successful"));
        }
        
    }
}
