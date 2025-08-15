using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Interfaces;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET: api/users/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { error = $"User with ID {id} not found" });
        }
        return Ok(user);
    }

    // GET: api/users/5/exists
    [HttpGet("{id:int}/exists")]
    public async Task<ActionResult> CheckUserExists(int id)
    {
        bool exists = await _userService.UserExistsAsync(id);
        return Ok(new { exists, userId = id });
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User? user)
    {
        var result = await _userService.CreateUserAsync(user);
        if (!result.success)
        {
            return BadRequest(new { error = result.errorMessage });
        }

        var createdUser = result.user!;
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
    }

    // PUT: api/users/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<User>> UpdateUser(int id, User? user)
    {
        var result = await _userService.UpdateUserAsync(id, user);
        if (!result.success)
        {
            return BadRequest(new { error = result.errorMessage });
        }

        return Ok(result.user);
    }

    // DELETE: api/users/5
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result.success)
        {
            return NotFound(new { error = result.errorMessage });
        }

        return NoContent();
    }
}
