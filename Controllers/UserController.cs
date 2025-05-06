using Microsoft.AspNetCore.Mvc;
using PromotionTasksService.Models;
using PromotionTasksService.Services;

namespace PromotionTasksService.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    /// <summary>
    /// Gets the user service.
    /// </summary>
    private readonly UserService userService;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<UserController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger.</param>
    public UserController(UserService userService, ILogger<UserController> logger)
    {
        this.userService = userService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>A list of all users.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        try
        {
            var users = await this.userService.GetAllUsersAsync();
            return this.Ok(users);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting all users");
            return this.StatusCode(500, "An error occurred while retrieving users");
        }
    }

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The user if found, or NotFound if not found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        try
        {
            var user = await this.userService.GetUserByIdWithReleasesAndTasksAsync(id);
            
            if (user == null)
            {
                return this.NotFound($"User with ID {id} not found");
            }

            return this.Ok(user);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting user by ID {UserId}", id);
            return this.StatusCode(500, "An error occurred while retrieving the user");
        }
    }
    
    /// <summary>
    /// Gets a user by their name.
    /// Note: This endpoint is provided for demo purposes only and should not be used in production
    /// as it bypasses proper authentication mechanisms.
    /// </summary>
    /// <param name="name">The name of the user to retrieve.</param>
    /// <returns>The user if found, or NotFound if not found.</returns>
    [HttpGet("name/{name}")]
    public async Task<ActionResult<User>> GetUserByName(string name)
    {
        try
        {
            var user = await this.userService.GetUserByNameWithReleasesAndTasksAsync(name);
            
            if (user == null)
            {
                return this.NotFound($"User with name '{name}' not found");
            }

            return this.Ok(user);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting user by name {UserName}", name);
            return this.StatusCode(500, "An error occurred while retrieving the user");
        }
    }
} 