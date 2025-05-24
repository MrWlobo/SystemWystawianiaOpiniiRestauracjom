using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Cuisine
{
    public int CuisineId { get; set; }
    public string? CuisineName { get; set; }
    public ICollection<Restaurant>? Restaurants { get; set; } 
}
}
