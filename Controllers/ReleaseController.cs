using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromotionTasksService.Models;
using PromotionTasksService.Services;

namespace PromotionTasksService.Controllers;

/// <summary>
/// Controller for managing releases.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReleaseController : ControllerBase
{
    /// <summary>
    /// Gets the release service.
    /// </summary>
    private readonly ReleaseService releaseService;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<ReleaseController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleaseController"/> class.
    /// </summary>
    /// <param name="releaseService">The release service.</param>
    /// <param name="logger">The logger.</param>
    public ReleaseController(ReleaseService releaseService, ILogger<ReleaseController> logger)
    {
        this.releaseService = releaseService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a release by its ID.
    /// </summary>
    /// <param name="id">The ID of the release to retrieve.</param>
    /// <returns>The release if found, or NotFound if not found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Release>> GetRelease(int id)
    {
        try
        {
            this.logger.LogInformation("Getting release with ID: {ReleaseId}", id);
            var release = await this.releaseService.GetReleaseByIdAsync(id);

            if (release == null)
            {
                this.logger.LogWarning("Release with ID: {ReleaseId} not found", id);
                return this.NotFound($"Release with ID {id} not found");
            }

            return this.Ok(release);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while getting release with ID: {ReleaseId}", id);
            return this.StatusCode(500, "An error occurred while retrieving the release");
        }
    }

    /// <summary>
    /// Gets all releases for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose releases to retrieve.</param>
    /// <returns>A list of releases for the specified user.</returns>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Release>>> GetUserReleases(int userId)
    {
        try
        {
            this.logger.LogInformation("Getting releases for user ID: {UserId}", userId);
            var releases = await this.releaseService.GetUserReleasesAsync(userId);
            return this.Ok(releases);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while getting releases for user ID: {UserId}", userId);
            return this.StatusCode(500, "An error occurred while retrieving user releases");
        }
    }

    /// <summary>
    /// Creates a new release.
    /// </summary>
    /// <param name="release">The release to create.</param>
    /// <returns>The created release.</returns>
    [HttpPost]
    public async Task<ActionResult<Release>> CreateRelease(Release release)
    {
        try
        {
            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Invalid release data provided");
                return this.BadRequest(this.ModelState);
            }

            this.logger.LogInformation("Creating new release for user ID: {UserId}", release.UserId);
            var createdRelease = await this.releaseService.CreateReleaseAsync(release);

            return this.CreatedAtAction(nameof(this.GetRelease), new { id = createdRelease.ReleaseId }, createdRelease);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while creating release for user ID: {UserId}", release.UserId);
            return this.StatusCode(500, "An unexpected error occurred");
        }
    }

    /// <summary>
    /// Updates an existing release.
    /// </summary>
    /// <param name="id">The ID of the release to update.</param>
    /// <param name="release">The updated release data.</param>
    /// <returns>NoContent if successful, or NotFound if the release doesn't exist.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelease(int id, Release release)
    {
        try
        {
            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Invalid release data provided for update");
                return this.BadRequest(this.ModelState);
            }

            if (id != release.ReleaseId)
            {
                this.logger.LogWarning("ID mismatch in update request. Path ID: {PathId}, Release ID: {ReleaseId}", id, release.ReleaseId);
                return this.BadRequest("ID mismatch");
            }

            var updatedRelease = await this.releaseService.UpdateReleaseAsync(id, release);
            if (updatedRelease == null)
            {
                this.logger.LogWarning("Release with ID: {ReleaseId} not found for update", id);
                return this.NotFound($"Release with ID {id} not found");
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while updating release with ID: {ReleaseId}", id);
            return this.StatusCode(500, "An unexpected error occurred while updating the release");
        }
    }

    /// <summary>
    /// Deletes a release.
    /// </summary>
    /// <param name="id">The ID of the release to delete.</param>
    /// <returns>NoContent if successful, or NotFound if the release doesn't exist.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelease(int id)
    {
        try
        {
            this.logger.LogInformation("Deleting release with ID: {ReleaseId}", id);
            var success = await this.releaseService.DeleteReleaseAsync(id);
            if (!success)
            {
                this.logger.LogWarning("Release with ID: {ReleaseId} not found for deletion", id);
                return this.NotFound($"Release with ID {id} not found");
            }

            return this.NoContent();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while deleting release with ID: {ReleaseId}", id);
            return this.StatusCode(500, "An error occurred while deleting the release");
        }
    }
} 