using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTOs;
using Models;

// ... (Make sure to include your DTOs here or in a shared DTOs namespace) ...

[ApiController]
[Route("api/[controller]")] // This will make the base route "api/Cuisines"
public class CuisinesController : ControllerBase
{
    private readonly ContextDb _context; // Your EF Core DbContext

    public CuisinesController(ContextDb context)
    {
        _context = context;
    }

    // POST: api/Cuisines
    // Adds a new cuisine
    [HttpPost]
    public async Task<ActionResult<CuisineDto>> AddCuisine([FromBody] AddCuisineDto addCuisineDto)
    {
        if (string.IsNullOrWhiteSpace(addCuisineDto.CuisineName))
        {
            return BadRequest("Cuisine name is required.");
        }

        // Optional: Check if a cuisine with the same name already exists to prevent duplicates
        var existingCuisine = await _context.Cuisines
            .FirstOrDefaultAsync(c => c.CuisineName != null && c.CuisineName.ToLower() == addCuisineDto.CuisineName.ToLower());

        if (existingCuisine != null)
        {
            return Conflict($"Cuisine with name '{addCuisineDto.CuisineName}' already exists.");
        }

        var newCuisine = new Cuisine
        {
            CuisineName = addCuisineDto.CuisineName
        };

        _context.Cuisines.Add(newCuisine);
        await _context.SaveChangesAsync();

        var cuisineDto = new CuisineDto
        {
            CuisineId = newCuisine.CuisineId,
            CuisineName = newCuisine.CuisineName
        };

        // CreatedAtAction returns a 201 Created status and includes a Location header
        // pointing to the newly created resource, and the resource itself in the body.
        return CreatedAtAction(nameof(GetCuisineById), new { id = cuisineDto.CuisineId }, cuisineDto);
    }

    // GET: api/Cuisines
    // Gets all cuisines
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CuisineDto>>> GetAllCuisines()
    {
        var cuisines = await _context.Cuisines.ToListAsync();

        var cuisineDtos = cuisines.Select(c => new CuisineDto
        {
            CuisineId = c.CuisineId,
            CuisineName = c.CuisineName
        }).ToList();

        return Ok(cuisineDtos);
    }

    // GET: api/Cuisines/{id}
    // Helper method for CreatedAtAction in AddCuisine, also useful for direct lookup
    [HttpGet("{id}")]
    public async Task<ActionResult<CuisineDto>> GetCuisineById(int id)
    {
        var cuisine = await _context.Cuisines.FindAsync(id);

        if (cuisine == null)
        {
            return NotFound();
        }

        var cuisineDto = new CuisineDto
        {
            CuisineId = cuisine.CuisineId,
            CuisineName = cuisine.CuisineName
        };

        return Ok(cuisineDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCuisine(int id)
    {
        var cuisine = await _context.Cuisines.FindAsync(id);

        if (cuisine == null)
        {
            return NotFound($"Cuisine with ID {id} not found.");
        }

        // Before deleting the cuisine, you might want to explicitly handle associated restaurants
        // In your DbContext, you've set DeleteBehavior.SetNull, meaning EF Core will automatically
        // set CuisineId to null for any restaurants linked to this cuisine when it's deleted.
        // If you wanted to, for example, prevent deletion if restaurants are linked, you'd do something like:
        /*
        var restaurantsWithCuisine = await _context.Restaurants.AnyAsync(r => r.CuisineId == id);
        if (restaurantsWithCuisine)
        {
            return BadRequest($"Cannot delete cuisine {cuisine.CuisineName} because there are restaurants associated with it. Please update or delete the restaurants first.");
        }
        */

        _context.Cuisines.Remove(cuisine);
        await _context.SaveChangesAsync();

        // Returning 204 No Content is a common successful response for DELETE operations
        return NoContent();
    }
}