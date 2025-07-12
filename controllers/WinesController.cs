using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WadesWineWatcher.Models;
using wwwbackend.data;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace WadesWineWatcher.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class WinesController : ControllerBase
    {
        private readonly WineDbContext _context;

        public WinesController(WineDbContext context)
        {
            _context = context;
        }


        // Returns all user wines
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var wines = await _context.Wines.ToListAsync();

            var userWines = wines
                .Where(w =>
                {
                    var userIds = JsonSerializer.Deserialize<List<int>>(w.UsersJson ?? "[]");
                    return userIds?.Contains(userId) ?? false;
                })
                .Select(w => new WineDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    StartDate = w.StartDate,
                    StartSpecificGravity = w.StartSpecificGravity,
                    EndSpecificGravity = w.EndSpecificGravity,
                    Ingredients = JsonSerializer.Deserialize<List<string>>(w.IngredientsJson ?? "[]") ?? new List<string>(),
                    RackDates = JsonSerializer.Deserialize<List<DateTime>>(w.RackDatesJson ?? "[]") ?? new List<DateTime>()
                })
                .ToList();

            return Ok(userWines);
        }


        // POST: api/wines
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddWine([FromBody] Wine wine)
        {
            // Get user ID from JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            // Set default values
            wine.IngredientsJson ??= "[]";
            wine.RackDatesJson ??= "[]";
            wine.UsersJson ??= "[]";

            // Add this user to UsersJson
            var usersList = System.Text.Json.JsonSerializer.Deserialize<List<int>>(wine.UsersJson) ?? new List<int>();
            if (!usersList.Contains(userId))
                usersList.Add(userId);

            wine.UsersJson = System.Text.Json.JsonSerializer.Serialize(usersList);

            _context.Wines.Add(wine);
            await _context.SaveChangesAsync();

            // Prepare DTO to return
            var wineDto = new WineDto
            {
                Id = wine.Id,
                Name = wine.Name,
                Description = wine.Description,
                StartDate = wine.StartDate,
                StartSpecificGravity = wine.StartSpecificGravity,
                EndSpecificGravity = wine.EndSpecificGravity,
                Ingredients = System.Text.Json.JsonSerializer.Deserialize<List<string>>(wine.IngredientsJson) ?? new List<string>(),
                RackDates = System.Text.Json.JsonSerializer.Deserialize<List<DateTime>>(wine.RackDatesJson) ?? new List<DateTime>()
            };

            return CreatedAtAction(nameof(Get), new { userId = userId }, wineDto);
        }

        public class WineDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public string Description { get; set; } = default!;
            public DateTime StartDate { get; set; }
            public double StartSpecificGravity { get; set; }
            public double EndSpecificGravity { get; set; }
            public List<string> Ingredients { get; set; } = new();
            public List<DateTime> RackDates { get; set; } = new();
        }

    }
}
