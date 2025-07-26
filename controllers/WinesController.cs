using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WadesWineWatcher.Models;
using wwwbackend.data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace wwwbackend.Controllers
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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var userWinesRaw = await _context.Wines
                .Where(w => w.WineUsers.Any(wu => wu.UserId == userId))
                .ToListAsync();

            var userWines = userWinesRaw.Select(w => new WineDto
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                StartDate = w.StartDate,
                StartSpecificGravity = w.StartSpecificGravity,
                EndSpecificGravity = w.EndSpecificGravity,
                Ingredients = string.IsNullOrWhiteSpace(w.Ingredients) ? new List<string>() : w.Ingredients.Split(',').ToList(),
                RackDates = string.IsNullOrWhiteSpace(w.RackDates) ? new List<DateTime>() : w.RackDates.Split(',').Select(DateTime.Parse).ToList()
            }).ToList();

            return Ok(userWines);
        }

        [Authorize]
        [HttpPost("addwine")]
        public async Task<IActionResult> AddWine([FromBody] Wine wine)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            _context.Wines.Add(wine);
            await _context.SaveChangesAsync();

            // Link user
            var wineUser = new WineUser
            {
                WineId = wine.Id,
                UserId = userId
            };
            _context.WineUsers.Add(wineUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = wine.Id }, wine);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditWine([FromBody] WineDto wineDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var wineOwned = await _context.Wines
                .Include(w => w.WineUsers)
                .FirstOrDefaultAsync(w => w.Id == wineDto.Id);

            if (wineOwned == null)
                return NotFound();

            if (!wineOwned.WineUsers.Any(wu => wu.UserId == userId))
                return Forbid();

            wineOwned.Name = wineDto.Name;
            wineOwned.Description = wineDto.Description;
            wineOwned.StartDate = wineDto.StartDate;
            wineOwned.StartSpecificGravity = wineDto.StartSpecificGravity;
            wineOwned.EndSpecificGravity = wineDto.EndSpecificGravity;

            // Convert arrays to comma-separated strings
            wineOwned.Ingredients = wineDto.Ingredients != null
                ? string.Join(",", wineDto.Ingredients)
                : "";

            wineOwned.RackDates = wineDto.RackDates != null
                ? string.Join(",", wineDto.RackDates.Select(d => d.ToString("o")))
                : "";

            await _context.SaveChangesAsync();

            return Ok(wineOwned);
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
