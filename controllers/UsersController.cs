using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wwwbackend.data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using System.Threading.Tasks;
using WadesWineWatcher.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly WineDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(WineDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            Console.WriteLine("Login Called");

            // Find matching username in DB
            var user = await _context.Users 
                .FirstOrDefaultAsync(u =>
                    u.username == loginRequest.Username);

            // Verify input pw with stored hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.password))
                return Unauthorized();

            // Defines info included in JWT
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Check Enviro for JWT Key
            var jwtSecret = _configuration["JWT_SECRET"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new Exception("JWT_SECRET is not set in configuration!");
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            // Create JWT with issuer, audience, expiration, claims, and signing key
            var token = new JwtSecurityToken(
                issuer: "WadeWineAPI",
                audience: "WadeWineClient",
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Returns expiration and token
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }

        [HttpGet("test")]
        public IActionResult Test() => Ok("API is running");

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
        {
            Console.WriteLine("Signup Called");

            // Check if username already exists
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.username == signUpRequest.Username
                );

            // if user exists, dont allow
            if (user != null)
            {
                return Unauthorized(new { message = "User already exists"});
            }

            if (!IsValidEmail(signUpRequest.Email))
            {
                return BadRequest(new { message = "Invalid email address" });
            }

            // Hash pw
            var hash = BCrypt.Net.BCrypt.HashPassword(signUpRequest.Password);

            // Make new user
            var newUser = new User
            {
                username = signUpRequest.Username,
                email = signUpRequest.Email,
                password = hash,
            };

            // Adds user to db, waits for confirmation
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Defines info included in JWT
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.id.ToString()),
                new Claim(ClaimTypes.Name, newUser.username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Check Enviro for JWT Key
            var jwtSecret = _configuration["JWT_SECRET"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new Exception("JWT_SECRET is not set in configuration!");
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            // Create JWT with issuer, audience, expiration, claims, and signing key
            var token = new JwtSecurityToken(
                issuer: "WadeWineAPI",
                audience: "WadeWineClient",
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Returns expiration and token
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });

        }


        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class SignUpRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
