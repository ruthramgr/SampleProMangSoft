using APIPMSoftware.Src.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace APIPMSoftware.Src.WebApi.Controller
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("GetDashboard")]
        public IActionResult GetDashboard()
        {
            var userJson = HttpContext.Items["SUser"] as string;

            if (userJson == null)
                return Unauthorized();

            var user = JsonSerializer.Deserialize<User>(userJson);

            return Ok($"Welcome {user.UserId}, Dashboard loaded");
        }
    }
}
