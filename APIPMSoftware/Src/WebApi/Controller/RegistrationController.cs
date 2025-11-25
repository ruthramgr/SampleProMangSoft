using APIPMSoftware.Src.Application.Common;
using APIPMSoftware.Src.Application.DTO;
using APIPMSoftware.Src.Application.Interface;
using APIPMSoftware.Src.Application.Service;
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
        private readonly IEmailService _emailService;
        public RegistrationController(
            IMemoryCache cashe,
            IUserRegistryRepository userRegistryRepo,
            IEmailService emailService)
        {
            _cashe=cashe;
            _userRegistryRepo =userRegistryRepo;
            _emailService = emailService;
        }
       
        [HttpPost("register/email")]
        public async Task<IActionResult> SubmitEmail([FromBody]Register request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(ResponseApiDynamic<string>.Fail("Email is required."));
                int userId = await _userRegistryRepo.CreateUserAsync(request.UserName,request.Email,request.CompanyName);
                string code = new Random().Next(100000, 999999).ToString();
                _cashe.Set("$RegistrationCode_" + userId, code, TimeSpan.FromMinutes(5));
                var result = await _emailService.SendVerificationEmailAsync(request.Email, code);
                if(result)
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
            return Ok();
        }
    }
}
