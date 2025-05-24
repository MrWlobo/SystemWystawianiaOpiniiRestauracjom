using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DTOs;
using Models;



[ApiController]
[Route("api/[controller]")] 
public class CuisinesController : ControllerBase
{
    private readonly ContextDb _context; 

    public CuisinesController(ContextDb context)
    {
        _context = context;
    }

    
    
    [HttpPost]
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<CuisineDto>> AddCuisine([FromBody] AddCuisineDto addCuisineDto)
    {
        if (string.IsNullOrWhiteSpace(addCuisineDto.CuisineName))
        {
            return BadRequest("Cuisine name is required.");
        }

        
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

        
        
        return CreatedAtAction(nameof(GetCuisineById), new { id = cuisineDto.CuisineId }, cuisineDto);
    }

    
    
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
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteCuisine(int id)
    {
        var cuisine = await _context.Cuisines.FindAsync(id);

        if (cuisine == null)
        {
            return NotFound($"Cuisine with ID {id} not found.");
        }
        _context.Cuisines.Remove(cuisine);
        await _context.SaveChangesAsync();

        
        return NoContent();
    }
}