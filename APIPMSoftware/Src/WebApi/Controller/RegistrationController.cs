using APIPMSoftware.Src.Application.Common;
using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Caching.Memory;

namespace APIPMSoftware.Src.WebApi.Controllers 
{
    [ApiController]
    [Route("api/registration")]
    public class RegistrationController : ControllerBase
    {
        private readonly IMemoryCache _cashe;
        private readonly IUserRegistryRepository _userRegistryRepo;
        private readonly IEmailSender _emailSender;

        public RegistrationController(
            IMemoryCache cashe,
            IUserRegistryRepository userRegistryRepo,         
            IEmailSender emailSender)
        {
            _cashe=cashe;
            _userRegistryRepo =userRegistryRepo;           
            _emailSender = emailSender;
        }
       
        [HttpPost("register/email")]
        public async Task<IActionResult> SubmitEmail([FromBody]Register request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(ResponseApiDynamic<string>.Fail("Email is required."));
                // User registration logic
                int userId = await _userRegistryRepo.CreateUserAsync(request.UserName,request.Email,request.CompanyName);

                string code = new Random().Next(100000, 999999).ToString();
                _cashe.Set("$RegistrationCode_" + userId, code, TimeSpan.FromMinutes(5));
                //var result = await _emailService.SendVerificationEmailAsync(request.Email, code);
                await _emailSender.SendAsync(request.Email, "🔐 Your Verification Code", code);
                var result = true; // Assume email sent successfully if no exception is thrown
                if (result)
                {
                    return Ok(ResponseApiDynamic<object>.Success(new { userId }, "Verification code sent to email."));
                }
                else
                {
                    return Ok(ResponseApiDynamic<object>.Success("Regisration done but faild to sent email."));
                }
            }
            catch (Exception ex)
            {

                return StatusCode(400, ResponseApiDynamic<string>.Fail($"Failed to register user: {ex.Message}"));
            }        
        }
        [HttpPost("register/resend-code")]
        public async Task<IActionResult> ResendCode([FromBody]ResendCodecs resendCodecs)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(resendCodecs.Email))
                    return BadRequest(ResponseApiDynamic<string>.Fail("Email is required."));
                var userId = await _userRegistryRepo.GetUserIdByEmail(resendCodecs.Email);
                string code = new Random().Next(100000,999999).ToString();
                _cashe.Set("$forgotCodeOTP" + userId, code, TimeSpan.FromMinutes(5));
                _cashe.Set("@RegistrationOTP" + userId, code, TimeSpan.FromMinutes(5));
                await _emailSender.SendAsync(resendCodecs.Email,"🔐 Your Verification Code", code);
                bool result = true; // Assume email sent successfully if no exception is thrown
                if(result)
                {
                    return Ok(ResponseApiDynamic<string>.Success("Verification code resent to email."));
                }
                else
                {
                    return Ok(ResponseApiDynamic<string>.Fail("Failed to resend verification code."));
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost("register/verify")]
        public IActionResult VerifyCode([FromBody] CodeVerify verify)
        {
            try
            {
                if(!_cashe.TryGetValue("$RegistrationOTP" + verify.userId,out string? storeCode))
                    return BadRequest(ResponseApiDynamic<string>.Fail("Verification code expired or not found."));
                if(storeCode !=verify.Code)
                    return BadRequest(ResponseApiDynamic<string>.Fail("Invalid verification code."));

                return Ok(ResponseApiDynamic<string>.Success("Email verified successfully."));
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
