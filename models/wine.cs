using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string IngredientsJson { get; set; } = "[]";
        public string RackDatesJson { get; set; } = "[]";
        public string UsersJson { get; set; } = "[]";
    }
}


