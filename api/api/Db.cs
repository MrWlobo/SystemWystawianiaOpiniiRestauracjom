public class Restaurant
{
    public int RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public Cuisine? Cuisine { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public int AddressId { get; set; }
    public string? City { get; set; }
    public string? Number { get; set; }
    public string? Street { get; set; }
    public Restaurant? Restaurant { get; set; }

}

public class Review
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public int? RestaurantId { get; set; }
    public int? Stars { get; set; }
    public string? Comment { get; set; }
    public User? User { get; set; }
}

public class User
{
    public int UserId { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public bool? isAdmin { get; set; }
    public ICollection<Review>? Reviews { get; set; }
}

public class Cuisine
{
    public int CuisineId { get; set; }
    public string? CuisineName { get; set; }
    public Restaurant? Restaurant { get; set; }
}