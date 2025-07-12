namespace WadesWineWatcher.Models
{
    public class Wine
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public List<string> Ingredients { get; set; } = default!;
        public string Description { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public List<DateTime> RackDates { get; set; } = default!;
        public List<int> Users { get; set; } = default!;
        public double StartSpecificGravity { get; set; }
        public double EndSpecificGravity { get; set; }
    }
}

