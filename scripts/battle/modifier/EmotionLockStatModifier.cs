namespace OmoriSandbox.Battle.Modifier;

public class EmotionLockStatModifier : StatModifier
{
    private string LockedEmotion;

    /// <summary>
    /// Represents a stat modifier that locks the actor's emotion to a specific one in damage calculations, in addition to applying stat bonuses.
    /// Mainly used for bosses that can lock their emotion
    /// </summary>
    /// <param name="lockedEmotion">The emotion this modifier locks to</param>
    /// <param name="bonuses">Additional stat bonuses, if any</param>
    public EmotionLockStatModifier(string lockedEmotion, params StatBonus[] bonuses) : base(bonuses)
    {
        LockedEmotion = lockedEmotion;
    }

    public EmotionLockStatModifier(string lockedEmotion, int turns, params StatBonus[] bonuses) : base(turns, bonuses)
    {
        LockedEmotion = lockedEmotion;
    }

    public override string OverrideEmotion()
    {
        return LockedEmotion;
    }
}