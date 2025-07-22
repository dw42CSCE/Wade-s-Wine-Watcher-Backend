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
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {

        private readonly WineDbContext _context;
        public EventsController(WineDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Addevent([FromBody] Event newEvent)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            newEvent.CreatorId = userId;

            var wineOwned = await _context.Wines
                .AnyAsync(w => w.Id == newEvent.WineId && w.WineUsers.Any(wu => wu.UserId == userId));

            if (!wineOwned)
                return Forbid(); // 403 Forbidden

            _context.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Addevent), new { id = newEvent.Id }, newEvent);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> EditEvent([FromBody] Event newEvent)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var wineOwned = await _context.Wines
                .AnyAsync(w => w.Id == newEvent.WineId && w.WineUsers.Any(wu => wu.UserId == userId));

            if (!wineOwned)
                return Forbid(); // 403 Forbidden

            var existingEvent = await _context.Events.FindAsync(newEvent.Id);
            if (existingEvent == null)
                return NotFound();

            if (existingEvent.CreatorId != userId)
                return Forbid();

            existingEvent.EventType = newEvent.EventType;
            existingEvent.EventDate = newEvent.EventDate;
            existingEvent.Description = newEvent.Description;

            await _context.SaveChangesAsync();
            return Ok(existingEvent);
        }

        [Authorize]
        [HttpGet]

        public async Task<IActionResult> GetEvents([FromQuery] int WineId)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var wineOwned = await _context.Wines
                .AnyAsync(w => w.Id == WineId && w.WineUsers.Any(wu => wu.UserId == userId));

            if (!wineOwned)
                return Forbid(); // 403 Forbidden

            var wineEventsRaw = await _context.Events
                .Where(w => w.WineId == WineId)
                .ToListAsync();

            var wineEvents = wineEventsRaw.Select(e => new EventDto
            {
                Id = e.Id,
                WineId = e.WineId,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Description = e.Description,
                CreatorId = e.CreatorId,
            }).ToList();

            return Ok(wineEvents);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteEvent([FromQuery] int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var e = await _context.Events.FindAsync(id);

            if (e == null) return NotFound();
            if (e.CreatorId != userId) return Forbid();

            _context.Events.Remove(e);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class EventDto
        {
            public int Id { get; set; }
            public int WineId { get; set; }

            public string EventType { get; set; } = string.Empty;
            public DateTime EventDate { get; set; }
            public string Description { get; set; } = string.Empty;

            public int CreatorId { get; set; }
        }
    }
}