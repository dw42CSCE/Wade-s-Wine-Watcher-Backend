using System.ComponentModel.DataAnnotations;

namespace WadesWineWatcher.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        public int WineId { get; set; }
        public Wine? Wine { get; set; }  // Navigation property to Wine

        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public int CreatorId { get; set; }
        public User? Creator { get; set; } // Nav prop for Users
    }
}
