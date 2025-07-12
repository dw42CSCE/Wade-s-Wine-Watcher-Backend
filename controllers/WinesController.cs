using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WadesWineWatcher.Models;

namespace WadesWineWatcher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WinesController : ControllerBase
    {
        private readonly string dataFile = Path.Combine("Data", "wines.json");

        [HttpGet]
        public IActionResult Get([FromQuery] int userId)
        {
            var json = System.IO.File.ReadAllText(dataFile);
            var wines = JsonSerializer.Deserialize<List<Wine>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var userWines = wines != null
                ? wines.Where(w => w.Users.Contains(userId)).ToList()
                : [];
            return Ok(userWines);
        }
    }
}
