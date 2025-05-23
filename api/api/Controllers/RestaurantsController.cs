using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTOs;
using Models;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly ContextDb _context; // Your EF Core DbContext

    public RestaurantsController(ContextDb context)
    {
        _context = context;
    }

    // POST: api/Restaurants
    // Adds a new restaurant with its cuisine and address
    [HttpPost]
    public async Task<ActionResult<RestaurantDetailDto>> AddRestaurant([FromBody] AddRestaurantDto addRestaurantDto)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(addRestaurantDto.RestaurantName))
        {
            return BadRequest("Restaurant name is required.");
        }
        if (addRestaurantDto.CuisineId == null)
        {
            return BadRequest("Cuisine ID is required.");
        }

        // Check if Cuisine exists
        var cuisine = await _context.Cuisines.FindAsync(addRestaurantDto.CuisineId);
        if (cuisine == null)
        {
            return NotFound($"Cuisine with ID {addRestaurantDto.CuisineId} not found.");
        }

        var newAddress = new Address
        {
            City = addRestaurantDto.Address?.City,
            Number = addRestaurantDto.Address?.Number,
            Street = addRestaurantDto.Address?.Street
        };

        var newRestaurant = new Restaurant
        {
            RestaurantName = addRestaurantDto.RestaurantName,
            CuisineId = addRestaurantDto.CuisineId, // Assign the foreign key
            Cuisine = cuisine, // Assign the navigation property (optional for add, but good practice if you want to use it immediately)
            Address = newAddress // Assign the related address
        };

        _context.Restaurants.Add(newRestaurant);
        await _context.SaveChangesAsync();

        // Load related data for the response DTO
        // We need to re-query to ensure all relationships are loaded correctly after save
        var createdRestaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .FirstOrDefaultAsync(r => r.RestaurantId == newRestaurant.RestaurantId);

        if (createdRestaurant == null) // Should not happen if save was successful
        {
            return StatusCode(500, "Failed to retrieve the newly created restaurant.");
        }

        var restaurantDetailDto = new RestaurantDetailDto
        {
            RestaurantId = createdRestaurant.RestaurantId,
            RestaurantName = createdRestaurant.RestaurantName,
            Cuisine = createdRestaurant.Cuisine != null ? new CuisineDto
            {
                CuisineId = createdRestaurant.Cuisine.CuisineId,
                CuisineName = createdRestaurant.Cuisine.CuisineName
            } : null,
            Address = createdRestaurant.Address != null ? new AddressDto
            {
                AddressId = createdRestaurant.Address.AddressId,
                City = createdRestaurant.Address.City,
                Number = createdRestaurant.Address.Number,
                Street = createdRestaurant.Address.Street
            } : null,
            Reviews = new List<ReviewDto>() // No reviews yet for a new restaurant
        };

        return CreatedAtAction(nameof(GetRestaurantById), new { id = restaurantDetailDto.RestaurantId }, restaurantDetailDto);
    }

    // GET: api/Restaurants
    // Gets all restaurants with their cuisines, reviews, and addresses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RestaurantListDto>>> GetAllRestaurants()
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            // .Include(r => r.Reviews) // Potentially heavy for a list, consider if really needed
            // .ThenInclude(review => review.User) // If you want user info in reviews for the list
            .ToListAsync();

        var restaurantListDtos = restaurants.Select(r => new RestaurantListDto
        {
            RestaurantId = r.RestaurantId,
            RestaurantName = r.RestaurantName,
            Cuisine = r.Cuisine != null ? new CuisineDto
            {
                CuisineId = r.Cuisine.CuisineId,
                CuisineName = r.Cuisine.CuisineName
            } : null,
            Address = r.Address != null ? new AddressDto
            {
                AddressId = r.Address.AddressId,
                City = r.Address.City,
                Number = r.Address.Number,
                Street = r.Address.Street
            } : null
            // For a list, you might want aggregated review data
            // ReviewCount = r.Reviews?.Count ?? 0,
            // AverageRating = r.Reviews != null && r.Reviews.Any(rev => rev.Stars.HasValue) ? r.Reviews.Average(rev => rev.Stars!.Value) : 0
        }).ToList();

        return Ok(restaurantListDtos);
    }

    // GET: api/Restaurants/{id}
    // Gets a single restaurant by id with its cuisine, address, and reviews
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDetailDto>> GetRestaurantById(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .Include(r => r.Reviews)
                .ThenInclude(review => review.User) // Eagerly load the User for each Review
            .FirstOrDefaultAsync(r => r.RestaurantId == id);

        if (restaurant == null)
        {
            return NotFound();
        }

        var restaurantDetailDto = new RestaurantDetailDto
        {
            RestaurantId = restaurant.RestaurantId,
            RestaurantName = restaurant.RestaurantName,
            Cuisine = restaurant.Cuisine != null ? new CuisineDto
            {
                CuisineId = restaurant.Cuisine.CuisineId,
                CuisineName = restaurant.Cuisine.CuisineName
            } : null,
            Address = restaurant.Address != null ? new AddressDto
            {
                AddressId = restaurant.Address.AddressId,
                City = restaurant.Address.City,
                Number = restaurant.Address.Number,
                Street = restaurant.Address.Street
            } : null,
            Reviews = restaurant.Reviews?.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Stars = r.Stars,
                Comment = r.Comment,
                User = r.User != null ? new UserDto
                {
                    UserId = r.User.UserId,
                    Login = r.User.Login
                } : null
            }).ToList() ?? new List<ReviewDto>()
        };

        return Ok(restaurantDetailDto);
    }

    [HttpGet("byCuisine/{cuisineId}")]
    public async Task<ActionResult<IEnumerable<RestaurantListDto>>> GetRestaurantsByCuisineId(int cuisineId)
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .Where(r => r.CuisineId == cuisineId) // Filter by CuisineId
            .ToListAsync();

        if (!restaurants.Any())
        {
            // Optionally, check if the cuisine ID itself exists
            var cuisineExists = await _context.Cuisines.AnyAsync(c => c.CuisineId == cuisineId);
            if (!cuisineExists)
            {
                return NotFound($"Cuisine with ID {cuisineId} not found.");
            }
            return NotFound($"No restaurants found for cuisine with ID {cuisineId}.");
        }

        var restaurantListDtos = restaurants.Select(r => new RestaurantListDto
        {
            RestaurantId = r.RestaurantId,
            RestaurantName = r.RestaurantName,
            Cuisine = r.Cuisine != null ? new CuisineDto
            {
                CuisineId = r.Cuisine.CuisineId,
                CuisineName = r.Cuisine.CuisineName
            } : null,
            Address = r.Address != null ? new AddressDto
            {
                AddressId = r.Address.AddressId,
                City = r.Address.City,
                Number = r.Address.Number,
                Street = r.Address.Street
            } : null
        }).ToList();

        return Ok(restaurantListDtos);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RestaurantDetailDto>> EditRestaurant(int id, [FromBody] EditRestaurantDto editRestaurantDto)
    {
        if (id != editRestaurantDto.RestaurantId)
        {
            return BadRequest("Restaurant ID in URL does not match ID in request body.");
        }

        // 1. Find the existing restaurant, including its related entities
        var existingRestaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .FirstOrDefaultAsync(r => r.RestaurantId == id);

        if (existingRestaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        // 2. Update Restaurant properties
        if (!string.IsNullOrWhiteSpace(editRestaurantDto.RestaurantName))
        {
            existingRestaurant.RestaurantName = editRestaurantDto.RestaurantName;
        }

        // 3. Update Cuisine (if provided)
        if (editRestaurantDto.CuisineId.HasValue)
        {
            var cuisine = await _context.Cuisines.FindAsync(editRestaurantDto.CuisineId.Value);
            if (cuisine == null)
            {
                return NotFound($"Cuisine with ID {editRestaurantDto.CuisineId.Value} not found.");
            }
            existingRestaurant.CuisineId = editRestaurantDto.CuisineId.Value;
            existingRestaurant.Cuisine = cuisine; // Update navigation property if needed for immediate use
        }
        else if (editRestaurantDto.CuisineId == null && existingRestaurant.CuisineId.HasValue)
        {
            // If CuisineId is explicitly set to null in DTO, and it currently has a value
            // this will set the CuisineId to null. This relies on the nullable int?
            existingRestaurant.CuisineId = null;
            existingRestaurant.Cuisine = null;
        }

        // 4. Update Address (if provided in DTO)
        if (editRestaurantDto.Address != null)
        {
            if (existingRestaurant.Address == null)
            {
                // Restaurant currently has no address, create a new one
                var newAddress = new Address
                {
                    City = editRestaurantDto.Address.City,
                    Number = editRestaurantDto.Address.Number,
                    Street = editRestaurantDto.Address.Street,
                    RestaurantId = existingRestaurant.RestaurantId // Link to parent restaurant
                };
                _context.Addresses.Add(newAddress);
                existingRestaurant.Address = newAddress;
            }
            else
            {
                // Restaurant has an existing address, update its properties
                // Ensure the AddressId matches, though not strictly necessary for 1-1 mapped like this
                if (existingRestaurant.Address.AddressId != editRestaurantDto.Address.AddressId)
                {
                    // This indicates an attempt to change the associated address, not just update it.
                    // You might want to handle this differently (e.g., error, or delete old and add new)
                    // For now, let's assume if an Address DTO is present, we update the existing one.
                    // The AddressId in EditAddressDto is more for clarity or if you were mapping to an unrelated Address entity.
                }

                existingRestaurant.Address.City = editRestaurantDto.Address.City;
                existingRestaurant.Address.Number = editRestaurantDto.Address.Number;
                existingRestaurant.Address.Street = editRestaurantDto.Address.Street;
                // No need to set existingRestaurant.Address.RestaurantId as it's already linked
            }
        }


        await _context.SaveChangesAsync();

        // 5. Re-query the updated restaurant to ensure all changes and relationships are loaded for the response
        // This is good practice to ensure the DTO reflects the exact state after save
        var updatedRestaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .Include(r => r.Reviews)
                .ThenInclude(review => review.User)
            .FirstOrDefaultAsync(r => r.RestaurantId == id);

        if (updatedRestaurant == null) // Should not happen after a successful save
        {
            return StatusCode(500, "Failed to retrieve the updated restaurant.");
        }

        // 6. Map to RestaurantDetailDto for the response
        var restaurantDetailDto = new RestaurantDetailDto
        {
            RestaurantId = updatedRestaurant.RestaurantId,
            RestaurantName = updatedRestaurant.RestaurantName,
            Cuisine = updatedRestaurant.Cuisine != null ? new CuisineDto
            {
                CuisineId = updatedRestaurant.Cuisine.CuisineId,
                CuisineName = updatedRestaurant.Cuisine.CuisineName
            } : null,
            Address = updatedRestaurant.Address != null ? new AddressDto
            {
                AddressId = updatedRestaurant.Address.AddressId,
                City = updatedRestaurant.Address.City,
                Number = updatedRestaurant.Address.Number,
                Street = updatedRestaurant.Address.Street
            } : null,
            Reviews = updatedRestaurant.Reviews?.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Stars = r.Stars,
                Comment = r.Comment,
                User = r.User != null ? new UserDto
                {
                    UserId = r.User.UserId,
                    Login = r.User.Login
                } : null
            }).ToList() ?? new List<ReviewDto>()
        };

        return Ok(restaurantDetailDto);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRestaurant(int id)
    {
        // When deleting, you typically don't need to Include related entities
        // unless you need to perform additional logic based on them before deletion,
        // or if EF Core's change tracker needs them loaded for specific cascade behaviors.
        // For CascadeDelete configured relationships, finding the parent is often sufficient.
        var restaurant = await _context.Restaurants.FindAsync(id);

        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        // The configured DeleteBehavior will handle related Address and Reviews:
        // - For Address (one-to-one, CascadeDelete): The associated address will be deleted.
        // - For Reviews (one-to-many):
        //   If DeleteBehavior.Cascade is configured, reviews will be deleted.
        //   If DeleteBehavior.SetNull is configured, Review.RestaurantId will be set to null.
        //   If not configured, EF Core's default for optional dependents often results in SetNull.

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        // Returning 204 No Content is standard for a successful DELETE operation
        return NoContent();
    }
}