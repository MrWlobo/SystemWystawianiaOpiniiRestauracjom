using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Cuisine
{
    public int CuisineId { get; set; }
    public string? CuisineName { get; set; }
    // Remove the RestaurantId and Restaurant navigation property from Cuisine
    // as Cuisine will not "know" about individual restaurants directly in a many-to-one.
    // Instead, Restaurant will have a CuisineId.
    public ICollection<Restaurant>? Restaurants { get; set; } // Add a collection for the many-to-one relationship
}
}
