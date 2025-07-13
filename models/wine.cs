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

        [Column("ingredients")]
        public string IngredientsJson { get; set; } = "[]";

        [Column("rackDates")]
        public string RackDatesJson { get; set; } = "[]";

        [Column("users")]
        public string UsersJson { get; set; } = "[]";
    }
}
