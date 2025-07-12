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

        // GET: api/wines?userId=5
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int userId)
        {
            var wines = await _context.Wines.ToListAsync();

            var userWines = wines.Where(w =>
            {
                var userIds = JsonSerializer.Deserialize<List<int>>(w.UsersJson ?? "[]");
                return userIds?.Contains(userId) ?? false;
            }).ToList();

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
}
