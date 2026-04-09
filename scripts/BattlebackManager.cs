using System.Collections.Generic;
using Godot;

namespace OmoriSandbox;

/// <summary>
/// Manages loading and retrieving Battlebacks.
/// </summary>
public sealed partial class BattlebackManager : Node
{
    public static BattlebackManager Instance;

    private readonly SortedDictionary<string, IBattleback> Battlebacks = [];
    
    public override void _EnterTree()
    {
        Instance = this;
        
        foreach (string battleback in ResourceLoader.ListDirectory("res://assets/battlebacks"))
            Battlebacks.TryAdd(battleback.GetBaseName(), new StaticBattleback("res://assets/battlebacks/" + battleback));
    }

    internal void AddBattleback(string resourcePath)
    {
        string name = resourcePath.GetFile().GetBaseName();
        if (Battlebacks.ContainsKey(name))
        {
            GD.PushWarning($"Battleback '{name}' already exists, skipping.");
            return;
        }
        string ext = resourcePath.GetExtension();
        if (ext == "gif")
            Battlebacks.TryAdd(name, new AnimatedBattleback(resourcePath));
        else if (ext == "png")
            Battlebacks.TryAdd(name, new StaticBattleback(resourcePath));
        else
            GD.PrintErr("Invalid battleback filetype! Must be .gif or .png: " + resourcePath);
    }

    /// <summary>
    /// Tries to retrieve a battleback by name from the cache.
    /// </summary>
    /// <param name="name">The name of the battleback to retrieve.</param>
    /// <param name="battleback">The corresponding battleback if present, otherwise null.</param>
    /// <returns>True if the battleback exists, false if not.</returns>
    public bool TryGetBattleback(string name, out IBattleback battleback)
    {
        if (!Battlebacks.TryGetValue(name, out battleback))
        {
            GD.PrintErr("Unknown battleback:" + name);
            return false;
        }
        return true;
    }

    internal IEnumerable<string> GetAllBattlebacks()
    {
        return Battlebacks.Keys;
    }
}