using System.ComponentModel.DataAnnotations.Schema;


namespace Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int? CuisineId { get; set; } // Add a foreign key to Cuisine
        [ForeignKey("CuisineId")] // Specify the foreign key
        public Cuisine? Cuisine { get; set; } // Navigation property to Cuisine
        public Address? Address { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
