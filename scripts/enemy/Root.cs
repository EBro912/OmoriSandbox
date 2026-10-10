using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Root : Enemy
{
    public override string Name => "ROOT";
    public override Vector2 InfoBoxOffset => new(15, -390);
    // VANILLA BUG: ROOT's emotion entries have no DamageOffsetY, so their damage numbers drop to its feet
    public override Vector2 DamageNumberOffset => CurrentEmotion.Group == null ? new(0, -250) : Vector2.Zero;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/root.tres");
    protected override Stats Stats => new(1000, 500, 30, 35, 10, 10, 95);
    protected override string[] EquippedSkills => ["ROAttack", "RODoNothing", "ROHealPlant"];
    
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry" or "afraid";
    }

    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Id)
        {
            case "happy":
                return new BattleCommand(this, SelectAllEnemies(), Skills["ROHealPlant"]);
            case "sad":
                return new BattleCommand(this, this, Skills["RODoNothing"]);
            case "angry":
                return new BattleCommand(this, SelectTarget(), Skills["ROAttack"]);
            default:
                if (Roll() < 46)
                    return new BattleCommand(this, SelectTarget(), Skills["ROAttack"]);
                if (Roll() < 31)
                    return new BattleCommand(this, this, Skills["RODoNothing"]);
                return new BattleCommand(this, SelectAllEnemies(), Skills["ROHealPlant"]);
        }
    }
}