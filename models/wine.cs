using System.ComponentModel.DataAnnotations;

namespace WadesWineWatcher.Models
{
    public class Wine
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public double StartSpecificGravity { get; set; }
        public double EndSpecificGravity { get; set; }

        // Store as comma-separated string for simplicity; or move to separate table if needed
        public string Ingredients { get; set; } = "";
        public string RackDates { get; set; } = "";

        public ICollection<WineUser> WineUsers { get; set; } = new List<WineUser>();
    }
}
