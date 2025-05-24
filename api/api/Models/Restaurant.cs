using System.ComponentModel.DataAnnotations.Schema;


namespace Models
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int? CuisineId { get; set; } 
        [ForeignKey("CuisineId")] 
        public Cuisine? Cuisine { get; set; } 
        public Address? Address { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}
