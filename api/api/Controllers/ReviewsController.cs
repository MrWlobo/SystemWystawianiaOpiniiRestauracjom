using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DTOs;
using Models;
using Db;


[ApiController]
[Route("api/[controller]")] 
public class ReviewsController : ControllerBase
{
    private readonly ContextDb _context;

    public ReviewsController(ContextDb context)
    {
        _context = context;
    }

    
    
    [HttpPost]
        [Authorize(Policy = "UserPolicy")]
        public async Task<ActionResult<ReviewDto>> AddReview([FromBody] AddReviewDto addReviewDto)
        {
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                
                
                return Unauthorized("User ID not found in token or invalid.");
            }

            
            if (addReviewDto.Stars < 1 || addReviewDto.Stars > 5)
            {
                return BadRequest("Stars must be between 1 and 5.");
            }

            
            var restaurant = await _context.Restaurants.FindAsync(addReviewDto.RestaurantId);
            if (restaurant == null)
            {
                return NotFound($"Restaurant with ID {addReviewDto.RestaurantId} not found.");
            }

            
            var newReview = new Review
            {
                
                UserId = currentUserId,
                RestaurantId = addReviewDto.RestaurantId,
                Stars = addReviewDto.Stars,
                Comment = addReviewDto.Comment
            };

            
            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            
            
            var createdReview = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Restaurant) 
                .FirstOrDefaultAsync(r => r.ReviewId == newReview.ReviewId);

            if (createdReview == null)
            {
                return StatusCode(500, "Failed to retrieve the newly created review after saving.");
            }

            
            var reviewDto = new ReviewDto
            {
                ReviewId = createdReview.ReviewId,
                Stars = createdReview.Stars,
                Comment = createdReview.Comment,
                
                User = createdReview.User != null ? new UserDto
                {
                    UserId = createdReview.User.UserId,
                    Login = createdReview.User.Login 
                } : null,
                
            };

            
            return CreatedAtAction(nameof(GetReviewById), new { id = reviewDto.ReviewId }, reviewDto);
        }

    
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReviewById(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (review == null)
        {
            return NotFound();
        }

        var reviewDto = new ReviewDto
        {
            ReviewId = review.ReviewId,
            Stars = review.Stars,
            Comment = review.Comment,
            User = review.User != null ? new UserDto
            {
                UserId = review.User.UserId,
                Login = review.User.Login
            } : null
        };

        return Ok(reviewDto);
    }

    
    
    [HttpPut("{id}")]
    [Authorize(Policy = "UserPolicy")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
    {
        if (id != updateReviewDto.ReviewId)
        {
            return BadRequest("Review ID in URL does not match ID in request body.");
        }

        var existingReview = await _context.Reviews.FindAsync(id);

        if (existingReview == null)
        {
            return NotFound($"Review with ID {id} not found.");
        }

        
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized("User ID not found in token.");
            }

            
            bool isAdmin = User.IsInRole("Admin");

            
            
            if (!isAdmin && existingReview.UserId != currentUserId)
            {
                return Forbid(); 
            }

        
        if (updateReviewDto.Stars.HasValue)
        {
            if (updateReviewDto.Stars < 1 || updateReviewDto.Stars > 5)
            {
                return BadRequest("Stars must be between 1 and 5.");
            }
            existingReview.Stars = updateReviewDto.Stars.Value;
        }
        if (!string.IsNullOrWhiteSpace(updateReviewDto.Comment))
        {
            existingReview.Comment = updateReviewDto.Comment;
        }

        await _context.SaveChangesAsync();

        
        var updatedReview = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (updatedReview == null) 
        {
            return StatusCode(500, "Failed to retrieve the updated review.");
        }

        var reviewDto = new ReviewDto
        {
            ReviewId = updatedReview.ReviewId,
            Stars = updatedReview.Stars,
            Comment = updatedReview.Comment,
            User = updatedReview.User != null ? new UserDto
            {
                UserId = updatedReview.User.UserId,
                Login = updatedReview.User.Login
            } : null
        };

        return Ok(reviewDto);
    }

    
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "UserPolicy")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return NotFound($"Review with ID {id} not found.");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
        {
            return Unauthorized("User ID not found in token.");
        }

        bool isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin && review.UserId != currentUserId)
        {
            return Forbid();
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent(); 
    }

    [HttpGet("{restaurantId}/reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForRestaurant(int restaurantId)
    {
        
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.RestaurantId == restaurantId);
        if (!restaurantExists)
        {
            return NotFound($"Restaurant with ID {restaurantId} not found.");
        }

        
        var reviews = await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId)
            .Include(r => r.User) 
            .ToListAsync();

        if (!reviews.Any())
        {
            return Ok(new List<ReviewDto>()); 
            
            
        }

        
        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            ReviewId = r.ReviewId,
            Stars = r.Stars,
            Comment = r.Comment,
            User = r.User != null ? new UserDto
            {
                UserId = r.User.UserId,
                Login = r.User.Login
            } : null
        }).ToList();

        return Ok(reviewDtos);
    }
}