using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int? RestaurantId { get; set; }
        public int? Stars { get; set; }
        public string? Comment { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }
    }
}
