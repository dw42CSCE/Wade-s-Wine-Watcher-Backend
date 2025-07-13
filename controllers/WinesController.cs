using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WadesWineWatcher.Models;
// Add the following using if WineDBContext is in a different namespace
using wwwbackend.data; // Ensure this namespace contains the WineDBContext class
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace wwwbackend.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WinesController : ControllerBase
    {
        // Make sure the context class name matches your actual DbContext class
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
                    var usersJson = string.IsNullOrWhiteSpace(w.UsersJson) ? "[]" : w.UsersJson;
                    List<int> userIds;
                    try
                    {
                        userIds = JsonSerializer.Deserialize<List<int>>(usersJson) ?? new List<int>();
                    }
                    catch
                    {
                        userIds = new List<int>();
                    }
                    return userIds.Contains(userId);
                })
                .Select(w =>
                {
                    var ingredientsJson = string.IsNullOrWhiteSpace(w.IngredientsJson) ? "[]" : w.IngredientsJson;
                    var rackDatesJson = string.IsNullOrWhiteSpace(w.RackDatesJson) ? "[]" : w.RackDatesJson;

                    List<string> ingredients;
                    List<DateTime> rackDates;

                    try
                    {
                        ingredients = JsonSerializer.Deserialize<List<string>>(ingredientsJson) ?? new List<string>();
                    }
                    catch
                    {
                        ingredients = new List<string>();
                    }

                    try
                    {
                        rackDates = JsonSerializer.Deserialize<List<DateTime>>(rackDatesJson) ?? new List<DateTime>();
                    }
                    catch
                    {
                        rackDates = new List<DateTime>();
                    }

                    return new WineDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        Description = w.Description,
                        StartDate = w.StartDate,
                        StartSpecificGravity = w.StartSpecificGravity,
                        EndSpecificGravity = w.EndSpecificGravity,
                        Ingredients = ingredients,
                        RackDates = rackDates
                    };
                })
                .ToList();

            return Ok(userWines);
        }

        // POST: api/wines
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddWine([FromBody] Wine wine)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            // Initialize JSON properties if null or empty
            wine.IngredientsJson = string.IsNullOrWhiteSpace(wine.IngredientsJson) ? "[]" : wine.IngredientsJson;
            wine.RackDatesJson = string.IsNullOrWhiteSpace(wine.RackDatesJson) ? "[]" : wine.RackDatesJson;
            wine.UsersJson = string.IsNullOrWhiteSpace(wine.UsersJson) ? "[]" : wine.UsersJson;

            // Add current user ID to UsersJson list
            List<int> usersList;
            try
            {
                usersList = JsonSerializer.Deserialize<List<int>>(wine.UsersJson) ?? new List<int>();
            }
            catch
            {
                usersList = new List<int>();
            }

            if (!usersList.Contains(userId))
                usersList.Add(userId);

            wine.UsersJson = JsonSerializer.Serialize(usersList);

            _context.Wines.Add(wine);
            await _context.SaveChangesAsync();

            var wineDto = new WineDto
            {
                Id = wine.Id,
                Name = wine.Name,
                Description = wine.Description,
                StartDate = wine.StartDate,
                StartSpecificGravity = wine.StartSpecificGravity,
                EndSpecificGravity = wine.EndSpecificGravity,
                Ingredients = JsonSerializer.Deserialize<List<string>>(wine.IngredientsJson) ?? new List<string>(),
                RackDates = JsonSerializer.Deserialize<List<DateTime>>(wine.RackDatesJson) ?? new List<DateTime>()
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
