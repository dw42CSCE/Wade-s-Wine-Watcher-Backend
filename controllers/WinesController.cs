using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WadesWineWatcher.Models;
using wwwbackend.data;
using System.Text.Json;

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
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int userId)
        {
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
        public async Task<IActionResult> AddWine([FromBody] Wine wine)
        {
            wine.IngredientsJson ??= "[]";
            wine.RackDatesJson ??= "[]";
            wine.UsersJson ??= "[]";

            _context.Wines.Add(wine);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { userId = 0 }, wine); // Dummy userId
        }
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
