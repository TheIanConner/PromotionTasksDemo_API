using Microsoft.AspNetCore.Mvc;
using PromotionTasksService.Models;
using PromotionTasksService.Services;

namespace PromotionTasksService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdWithReleasesAndTasksAsync(id);
            
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {UserId}", id);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }
    
    /// <summary>
    /// Get a user by name
    /// This is not a good practice and should be avoided. For the purpose of the demo which has no authentication,
    /// I'm allowing the user to get a user by name.
    /// Note: Prefixing the route with 'name' to avoid conflict with the 'id' route.
    /// </summary>
    /// <param name="name">The name of the user to get</param>
    /// <returns>The user with the given name</returns>
    [HttpGet("name/{name}")]
    public async Task<ActionResult<User>> GetUserByName(string name)
    {
        try
        {
            var user = await _userService.GetUserByNameWithReleasesAndTasksAsync(name);
            
            if (user == null)
            {
                return NotFound($"User with name '{name}' not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by name {UserName}", name);
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }
} 