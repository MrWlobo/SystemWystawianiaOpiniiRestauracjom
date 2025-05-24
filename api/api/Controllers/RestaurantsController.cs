using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DTOs;
using Models;

[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly ContextDb _context; 

    public RestaurantsController(ContextDb context)
    {
        _context = context;
    }

    
    
    [HttpPost]
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<RestaurantDetailDto>> AddRestaurant([FromBody] AddRestaurantDto addRestaurantDto)
    {
        
        if (string.IsNullOrWhiteSpace(addRestaurantDto.RestaurantName))
        {
            return BadRequest("Restaurant name is required.");
        }
        if (addRestaurantDto.CuisineId == null)
        {
            return BadRequest("Cuisine ID is required.");
        }

        
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
            CuisineId = addRestaurantDto.CuisineId, 
            Cuisine = cuisine, 
            Address = newAddress 
        };

        _context.Restaurants.Add(newRestaurant);
        await _context.SaveChangesAsync();

        
        
        var createdRestaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .FirstOrDefaultAsync(r => r.RestaurantId == newRestaurant.RestaurantId);

        if (createdRestaurant == null) 
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
            Reviews = new List<ReviewDto>() 
        };

        return CreatedAtAction(nameof(GetRestaurantById), new { id = restaurantDetailDto.RestaurantId }, restaurantDetailDto);
    }

    
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RestaurantListDto>>> GetAllRestaurants()
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            
            
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
            
            
            
        }).ToList();

        return Ok(restaurantListDtos);
    }

    
    
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDetailDto>> GetRestaurantById(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .Include(r => r.Reviews)
                .ThenInclude(review => review.User) 
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
            .Where(r => r.CuisineId == cuisineId) 
            .ToListAsync();

        if (!restaurants.Any())
        {
            
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
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<RestaurantDetailDto>> EditRestaurant(int id, [FromBody] EditRestaurantDto editRestaurantDto)
    {
        if (id != editRestaurantDto.RestaurantId)
        {
            return BadRequest("Restaurant ID in URL does not match ID in request body.");
        }

        
        var existingRestaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .FirstOrDefaultAsync(r => r.RestaurantId == id);

        if (existingRestaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        
        if (!string.IsNullOrWhiteSpace(editRestaurantDto.RestaurantName))
        {
            existingRestaurant.RestaurantName = editRestaurantDto.RestaurantName;
        }

        
        if (editRestaurantDto.CuisineId.HasValue)
        {
            var cuisine = await _context.Cuisines.FindAsync(editRestaurantDto.CuisineId.Value);
            if (cuisine == null)
            {
                return NotFound($"Cuisine with ID {editRestaurantDto.CuisineId.Value} not found.");
            }
            existingRestaurant.CuisineId = editRestaurantDto.CuisineId.Value;
            existingRestaurant.Cuisine = cuisine; 
        }
        else if (editRestaurantDto.CuisineId == null && existingRestaurant.CuisineId.HasValue)
        {
            
            
            existingRestaurant.CuisineId = null;
            existingRestaurant.Cuisine = null;
        }

        
        if (editRestaurantDto.Address != null)
        {
            if (existingRestaurant.Address == null)
            {
                
                var newAddress = new Address
                {
                    City = editRestaurantDto.Address.City,
                    Number = editRestaurantDto.Address.Number,
                    Street = editRestaurantDto.Address.Street,
                    RestaurantId = existingRestaurant.RestaurantId 
                };
                _context.Addresses.Add(newAddress);
                existingRestaurant.Address = newAddress;
            }
            else
            {
                
                
                if (existingRestaurant.Address.AddressId != editRestaurantDto.Address.AddressId)
                {
                    
                    
                    
                    
                }

                existingRestaurant.Address.City = editRestaurantDto.Address.City;
                existingRestaurant.Address.Number = editRestaurantDto.Address.Number;
                existingRestaurant.Address.Street = editRestaurantDto.Address.Street;
                
            }
        }


        await _context.SaveChangesAsync();

        
        
        var updatedRestaurant = await _context.Restaurants
            .Include(r => r.Cuisine)
            .Include(r => r.Address)
            .Include(r => r.Reviews)
                .ThenInclude(review => review.User)
            .FirstOrDefaultAsync(r => r.RestaurantId == id);

        if (updatedRestaurant == null) 
        {
            return StatusCode(500, "Failed to retrieve the updated restaurant.");
        }

        
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
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteRestaurant(int id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);

        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        
        return NoContent();
    }
}