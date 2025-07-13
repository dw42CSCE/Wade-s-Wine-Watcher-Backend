namespace WadesWineWatcher.Models
{
    public class WineUser
    {
        public int WineId { get; set; }
        public Wine Wine { get; set; } = default!;

        public int UserId { get; set; }
        public User User { get; set; } = default!;
    }
}
