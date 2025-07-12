using Microsoft.AspNetCore.Mvc;
using backend.Models;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly List<User> _users;

        public UsersController()
        {
            var jsonData = System.IO.File.ReadAllText("data/users.json");
            _users = JsonSerializer.Deserialize<List<User>>(jsonData) ?? new List<User>();
        }

        // GET: api/users
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return Ok(_users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.id == id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        // POST: api/users/login
        [HttpPost("login")]
        public ActionResult<User> Login([FromBody] LoginRequest loginRequest)
        {
            var user = _users.FirstOrDefault(u => u.username == loginRequest.Username && u.password == loginRequest.Password);
            if (user == null)
            {
                Console.WriteLine($"Login attempt: {loginRequest.Username}, {loginRequest.Password}");
                return Unauthorized();
            }
            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
