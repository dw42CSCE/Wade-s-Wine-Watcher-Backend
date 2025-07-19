using System.ComponentModel.DataAnnotations;

namespace WadesWineWatcher.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }
        public string username { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;

        public ICollection<WineUser> WineUsers { get; set; } = new List<WineUser>();
        public ICollection<Event> EventsCreated { get; set; } = new List<Event>();
    }
}