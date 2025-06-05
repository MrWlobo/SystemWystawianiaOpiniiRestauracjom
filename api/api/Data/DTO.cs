using System.ComponentModel.DataAnnotations;

namespace DTOs
{
    public class AddAddressDto
    {
        public string? City { get; set; }
        public string? Number { get; set; }
        public string? Street { get; set; }
    }

    public class AddRestaurantDto
    {
        public string? RestaurantName { get; set; }
        public int? CuisineId { get; set; }
        public AddAddressDto? Address { get; set; }
    }

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
        public UserDto? User { get; set; }
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
    }

    public class AddCuisineDto
    {
        public string? CuisineName { get; set; }
    }

    public class EditRestaurantDto
    {
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; }
        public int? CuisineId { get; set; }
        public EditAddressDto? Address { get; set; }
    }

    public class EditAddressDto
    {
        public int AddressId { get; set; }
        public string? City { get; set; }
        public string? Number { get; set; }
        public string? Street { get; set; }
    }

    public class AddReviewDto
    {
        public int RestaurantId { get; set; }
        public int Stars { get; set; }
        public string? Comment { get; set; }
    }

    // DTO for updating an existing Review
    public class UpdateReviewDto
    {
        public int ReviewId { get; set; } 
        public int? Stars { get; set; } 
        public string? Comment { get; set; }
    }

    public class AddUserDto
    {
        public string? Login { get; set; }
        public string? Password { get; set; } 
        public bool? IsAdmin { get; set; } = false; 
    }

    // DTO for updating an existing User
    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public bool? IsAdmin { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Login1 { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string? Login { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
    }

    public class AuthResponse
    {
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string? Login { get; set; }
        public bool? IsAdmin { get; set; }
    }
}