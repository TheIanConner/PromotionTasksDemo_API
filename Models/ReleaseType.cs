namespace PromotionTasksService.Models;

/// <summary>
/// Defines the types of music releases available in the system.
/// </summary>
public enum ReleaseType
{
    /// <summary>
    /// A single track release.
    /// </summary>
    Single = 0,

    /// <summary>
    /// An extended play (EP) release, typically containing 3-6 tracks.
    /// </summary>
    EP,

    /// <summary>
    /// A full album release, typically containing 8 or more tracks.
    /// </summary>
    Album,

    /// <summary>
    /// A mixtape release, typically a collection of tracks released for free.
    /// </summary>
    Mixtape,
} 