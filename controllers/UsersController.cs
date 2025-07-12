using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using wwwbackend.data;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly WineDbContext _context;

        public UsersController(WineDbContext context)
        {
            _context = context;
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.username == loginRequest.Username &&
                    u.password == loginRequest.Password);

            if (user == null)
                return Unauthorized();

            // Only return 200 OK, no user data
            return Ok();
        }

    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
