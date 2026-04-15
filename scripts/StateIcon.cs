namespace OmoriSandbox;

/// <summary>
/// A struct containing info for State Icons
/// </summary>
public struct StateIcon
{
    /// <summary>
    /// The state icon's asset name
    /// </summary>
    public string AssetName { get; init; }
    /// <summary>
    /// The state icon's description, shown when hovered over with the mouse
    /// </summary>
    public string Description { get; init; }
    
    /// <summary>
    /// A struct containing info for State Icons
    /// </summary>
    /// <param name="assetName">The state icon's asset name</param>
    /// <param name="description">The state icon's description, shown when hovered over with the mouse</param>
    public StateIcon(string assetName, string description)
    {
        AssetName = assetName;
        Description = description;
    }
}