using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTOs;
using Models;

// ... (Make sure to include your DTOs here or in a shared DTOs namespace) ...

[ApiController]
[Route("api/[controller]")] // This will make the base route "api/Users"
public class UsersController : ControllerBase
{
    private readonly ContextDb _context;

    public UsersController(ContextDb context)
    {
        _context = context;
    }

    // POST: api/Users
    // Adds a new user
    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] AddUserDto addUserDto)
    {
        if (string.IsNullOrWhiteSpace(addUserDto.Login) || string.IsNullOrWhiteSpace(addUserDto.Password))
        {
            return BadRequest("Login and Password are required.");
        }

        // Check if user with same login already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Login != null && u.Login.ToLower() == addUserDto.Login.ToLower());

        if (existingUser != null)
        {
            return Conflict($"User with login '{addUserDto.Login}' already exists.");
        }

        var newUser = new User
        {
            Login = addUserDto.Login,
            Password = addUserDto.Password, // WARNING: Store hashed password in production!
            isAdmin = addUserDto.IsAdmin ?? false // Use the DTO value, or default to false
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        var userDto = new UserDto
        {
            UserId = newUser.UserId,
            Login = newUser.Login,
            IsAdmin = newUser.isAdmin
        };

        return CreatedAtAction(nameof(GetUserById), new { id = userDto.UserId }, userDto);
    }

    // GET: api/Users
    // Gets all users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();

        var userDtos = users.Select(u => new UserDto
        {
            UserId = u.UserId,
            Login = u.Login,
            IsAdmin = u.isAdmin
        }).ToList();

        return Ok(userDtos);
    }

    // GET: api/Users/{id}
    // Gets a single user by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var userDto = new UserDto
        {
            UserId = user.UserId,
            Login = user.Login,
            IsAdmin = user.isAdmin
        };

        return Ok(userDto);
    }


    // PUT: api/Users/{id}
    // Updates an existing user
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (id != updateUserDto.UserId)
        {
            return BadRequest("User ID in URL does not match ID in request body.");
        }

        var existingUser = await _context.Users.FindAsync(id);

        if (existingUser == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        // Update properties if provided in the DTO
        if (!string.IsNullOrWhiteSpace(updateUserDto.Login))
        {
            // Optional: Check for duplicate login if changing login
            if (updateUserDto.Login.ToLower() != existingUser.Login?.ToLower())
            {
                var loginExists = await _context.Users.AnyAsync(u => u.Login != null && u.Login.ToLower() == updateUserDto.Login.ToLower() && u.UserId != id);
                if (loginExists)
                {
                    return Conflict($"Login '{updateUserDto.Login}' is already taken by another user.");
                }
            }
            existingUser.Login = updateUserDto.Login;
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
        {
            existingUser.Password = updateUserDto.Password; // WARNING: Hash password in production!
        }

        if (updateUserDto.IsAdmin.HasValue)
        {
            existingUser.isAdmin = updateUserDto.IsAdmin.Value;
        }

        await _context.SaveChangesAsync();

        // Return the updated user's DTO
        var userDto = new UserDto
        {
            UserId = existingUser.UserId,
            Login = existingUser.Login,
            IsAdmin = existingUser.isAdmin
        };

        return Ok(userDto);
    }

    // DELETE: api/Users/{id}
    // Deletes a user
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content for successful deletion
    }
}