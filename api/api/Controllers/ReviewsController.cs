using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTOs;
using Models;



[ApiController]
[Route("api/[controller]")] // Route will be "api/Reviews"
public class ReviewsController : ControllerBase
{
    private readonly ContextDb _context;

    public ReviewsController(ContextDb context)
    {
        _context = context;
    }

    // POST: api/Reviews
    // Adds a new review
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> AddReview([FromBody] AddReviewDto addReviewDto)
    {
        // Validation
        if (addReviewDto.Stars < 1 || addReviewDto.Stars > 5)
        {
            return BadRequest("Stars must be between 1 and 5.");
        }

        // Check if User and Restaurant exist
        var user = await _context.Users.FindAsync(addReviewDto.UserId);
        if (user == null)
        {
            return NotFound($"User with ID {addReviewDto.UserId} not found.");
        }
        var restaurant = await _context.Restaurants.FindAsync(addReviewDto.RestaurantId);
        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {addReviewDto.RestaurantId} not found.");
        }

        var newReview = new Review
        {
            UserId = addReviewDto.UserId,
            RestaurantId = addReviewDto.RestaurantId,
            Stars = addReviewDto.Stars,
            Comment = addReviewDto.Comment
        };

        _context.Reviews.Add(newReview);
        await _context.SaveChangesAsync();

        // Load related data for the response DTO
        var createdReview = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == newReview.ReviewId);

        if (createdReview == null)
        {
            return StatusCode(500, "Failed to retrieve the newly created review.");
        }

        var reviewDto = new ReviewDto
        {
            ReviewId = createdReview.ReviewId,
            Stars = createdReview.Stars,
            Comment = createdReview.Comment,
            User = createdReview.User != null ? new UserDto
            {
                UserId = createdReview.User.UserId,
                Login = createdReview.User.Login // Only send necessary user info
            } : null
        };

        return CreatedAtAction(nameof(GetReviewById), new { id = reviewDto.ReviewId }, reviewDto);
    }

    // GET: api/Reviews/{id}
    // Helper method for CreatedAtAction and for getting a review by ID
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

    // PUT: api/Reviews/{id}
    // Updates an existing review
    [HttpPut("{id}")]
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

        // Update properties if provided in the DTO
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

        // Re-query for the updated review, including the User
        var updatedReview = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (updatedReview == null) // Should not happen after a successful save
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

    // DELETE: api/Reviews/{id}
    // Deletes a review
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
        {
            return NotFound($"Review with ID {id} not found.");
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content for successful deletion
    }

    [HttpGet("{restaurantId}/reviews")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsForRestaurant(int restaurantId)
    {
        // First, check if the restaurant itself exists
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.RestaurantId == restaurantId);
        if (!restaurantExists)
        {
            return NotFound($"Restaurant with ID {restaurantId} not found.");
        }

        // Retrieve reviews for the specified restaurant, including the User who wrote the review
        var reviews = await _context.Reviews
            .Where(r => r.RestaurantId == restaurantId)
            .Include(r => r.User) // Eagerly load the User for each review
            .ToListAsync();

        if (!reviews.Any())
        {
            return Ok(new List<ReviewDto>()); // Return an empty list if no reviews are found
            // Alternatively, you could return NotFound() if you consider a restaurant with no reviews as "not found" for this specific query,
            // but returning an empty list is generally more user-friendly for collection endpoints.
        }

        // Map the entities to DTOs
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