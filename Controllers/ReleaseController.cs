using Microsoft.AspNetCore.Mvc;
using PromotionTasksService.Models;
using PromotionTasksService.Services;
using Microsoft.Extensions.Logging;

namespace PromotionTasksService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReleaseController : ControllerBase
{
    private readonly ReleaseService _releaseService;
    private readonly ILogger<ReleaseController> _logger;

    public ReleaseController(ReleaseService releaseService, ILogger<ReleaseController> logger)
    {
        _releaseService = releaseService;
        _logger = logger;
    }

    // GET: api/Release/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Release>> GetRelease(int id)
    {
        try
        {
            _logger.LogInformation("Getting release with ID: {ReleaseId}", id);
            var release = await _releaseService.GetReleaseByIdAsync(id);

            if (release == null)
            {
                _logger.LogWarning("Release with ID: {ReleaseId} not found", id);
                return NotFound($"Release with ID {id} not found");
            }

            return Ok(release);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting release with ID: {ReleaseId}", id);
            return StatusCode(500, "An error occurred while retrieving the release");
        }
    }

    // GET: api/Release/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Release>>> GetUserReleases(int userId)
    {
        try
        {
            _logger.LogInformation("Getting releases for user ID: {UserId}", userId);
            var releases = await _releaseService.GetUserReleasesAsync(userId);
            return Ok(releases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting releases for user ID: {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving user releases");
        }
    }

    // POST: api/Release
    [HttpPost]
    public async Task<ActionResult<Release>> CreateRelease(Release release)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid release data provided");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Creating new release for user ID: {UserId}", release.UserId);
            var createdRelease = await _releaseService.CreateReleaseAsync(release);

            return CreatedAtAction(nameof(GetRelease), new { id = createdRelease.ReleaseId }, createdRelease);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating release for user ID: {UserId}", release.UserId);
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    // PUT: api/Release/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelease(int id, Release release)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid release data provided for update");
                return BadRequest(ModelState);
            }

            if (id != release.ReleaseId)
            {
                _logger.LogWarning("ID mismatch in update request. Path ID: {PathId}, Release ID: {ReleaseId}", id, release.ReleaseId);
                return BadRequest("ID mismatch");
            }

            var updatedRelease = await _releaseService.UpdateReleaseAsync(id, release);
            if (updatedRelease == null)
            {
                _logger.LogWarning("Release with ID: {ReleaseId} not found for update", id);
                return NotFound($"Release with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating release with ID: {ReleaseId}", id);
            return StatusCode(500, "An unexpected error occurred while updating the release");
        }
    }

    // DELETE: api/Release/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelease(int id)
    {
        try
        {
            _logger.LogInformation("Deleting release with ID: {ReleaseId}", id);
            var success = await _releaseService.DeleteReleaseAsync(id);
            if (!success)
            {
                _logger.LogWarning("Release with ID: {ReleaseId} not found for deletion", id);
                return NotFound($"Release with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting release with ID: {ReleaseId}", id);
            return StatusCode(500, "An error occurred while deleting the release");
        }
    }
} 