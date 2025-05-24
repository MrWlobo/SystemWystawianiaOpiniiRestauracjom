using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DTOs;
using Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ContextDb _context;

    public UsersController(ContextDb context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] AddUserDto addUserDto)
    {
        if (string.IsNullOrWhiteSpace(addUserDto.Login) || string.IsNullOrWhiteSpace(addUserDto.Password))
        {
            return BadRequest("Login and Password are required.");
        }

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Login != null && u.Login.ToLower() == addUserDto.Login.ToLower());

        if (existingUser != null)
        {
            return Conflict($"User with login '{addUserDto.Login}' already exists.");
        }

        var newUser = new User
        {
            Login = addUserDto.Login,
            Password = addUserDto.Password, 
            isAdmin = addUserDto.IsAdmin ?? false 
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

    
    
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
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


    
    
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
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

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
        {
            return Unauthorized("User ID not found in token.");
        }

        bool isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin && existingUser.UserId != currentUserId)
        {
            return Forbid();
        }

        
        if (!string.IsNullOrWhiteSpace(updateUserDto.Login))
        {
            
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
            existingUser.Password = updateUserDto.Password; 
        }

        if (updateUserDto.IsAdmin.HasValue)
        {
            existingUser.isAdmin = updateUserDto.IsAdmin.Value;
        }

        await _context.SaveChangesAsync();

        
        var userDto = new UserDto
        {
            UserId = existingUser.UserId,
            Login = existingUser.Login,
            IsAdmin = existingUser.isAdmin
        };

        return Ok(userDto);
    }

    
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent(); 
    }
}