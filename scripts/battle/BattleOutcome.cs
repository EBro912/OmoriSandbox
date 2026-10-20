namespace OmoriSandbox.Battle;

/// <summary>
/// How a battle ended. See <see cref="BattleEndedEventArgs.Outcome"/>.
/// </summary>
public enum BattleOutcome
{
    /// <summary>The party defeated every enemy.</summary>
    Victory,
    /// <summary>The party was defeated.</summary>
    Defeat,
    /// <summary>An enemy ended the battle early.</summary>
    Aborted
}
