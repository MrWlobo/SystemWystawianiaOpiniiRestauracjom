using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Address
    {
        public int AddressId { get; set; }
        public string? City { get; set; }
        public string? Number { get; set; }
        public string? Street { get; set; }
        public int RestaurantId { get; set; }

        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }

    }
}
