using System.Threading.Tasks;

namespace OmoriSandbox.Battle;

/// <summary>
/// An empty <see cref="BattleAction"/> that does nothing.
/// </summary>
public sealed class EmptyAction : BattleAction
{
    /// <summary>
    /// An empty <see cref="BattleAction"/> that does nothing.
    /// </summary>
    public EmptyAction() : base("EmptyAction", "Does nothing", SkillTarget.Self, 0,
        (_, self) => { BattleLogManager.Instance.QueueMessage(self, "[actor] does nothing."); return Task.CompletedTask; })
    { }
}