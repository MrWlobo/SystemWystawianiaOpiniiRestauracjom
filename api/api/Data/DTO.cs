namespace DTOs
{
    // DTOs for adding a Restaurant
    public class AddAddressDto
    {
        public string? City { get; set; }
        public string? Number { get; set; }
        public string? Street { get; set; }
    }

    public class AddRestaurantDto
    {
        public string? RestaurantName { get; set; }
        public int? CuisineId { get; set; } // Expect an existing Cuisine ID
        public AddAddressDto? Address { get; set; }
    }

    // DTOs for retrieving data (output)
    public class CuisineDto
    {
        public int CuisineId { get; set; }
        public string? CuisineName { get; set; }
    }

    public class AddressDto
    {
        public int AddressId { get; set; }
        public string? City { get; set; }
        public string? Number { get; set; }
        public string? Street { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string? Login { get; set; }
        public bool? IsAdmin { get; set; }
    }

    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int? Stars { get; set; }
        public string? Comment { get; set; }
        public UserDto? User { get; set; } // Include user who made the review
    }

    public class RestaurantDetailDto
    {
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public CuisineDto? Cuisine { get; set; }
        public AddressDto? Address { get; set; }
        public ICollection<ReviewDto>? Reviews { get; set; }
    }

    public class RestaurantListDto
    {
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public CuisineDto? Cuisine { get; set; }
        public AddressDto? Address { get; set; }
        // For a list, you might not want all reviews, just a count or average rating
        // public int ReviewCount { get; set; } // Example: if you want to show review count
        // public double AverageRating { get; set; } // Example: if you want to show average rating
    }

    public class AddCuisineDto
    {
        public string? CuisineName { get; set; }
    }

    public class EditRestaurantDto
    {
        public int RestaurantId { get; set; } // Needed to identify which restaurant to edit
        public string? RestaurantName { get; set; }
        public int? CuisineId { get; set; } // Allow changing cuisine
        public EditAddressDto? Address { get; set; } // DTO for updating address details
    }

    public class EditAddressDto
    {
        public int AddressId { get; set; } // Needed to identify which address to edit
        public string? City { get; set; }
        public string? Number { get; set; }
        public string? Street { get; set; }
    }

    public class AddReviewDto
    {
        public int UserId { get; set; } // The ID of the user creating the review
        public int RestaurantId { get; set; } // The ID of the restaurant being reviewed
        public int Stars { get; set; }
        public string? Comment { get; set; }
    }

    // DTO for updating an existing Review
    public class UpdateReviewDto
    {
        public int ReviewId { get; set; } // The ID of the review to update
        public int? Stars { get; set; } // Nullable to allow partial updates
        public string? Comment { get; set; } // Nullable to allow partial updates
    }

    public class AddUserDto
    {
        public string? Login { get; set; }
        public string? Password { get; set; } // WARNING: In a real app, this should be hashed before saving!
        public bool? IsAdmin { get; set; } = false; // Default to false if not provided
    }

    // DTO for updating an existing User
    public class UpdateUserDto
    {
        public int UserId { get; set; } // The ID of the user to update
        public string? Login { get; set; }
        public string? Password { get; set; } // WARNING: In a real app, handle securely (e.g., separate endpoint, hashing)
        public bool? IsAdmin { get; set; }
    }
}