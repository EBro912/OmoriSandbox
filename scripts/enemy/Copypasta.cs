using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Copypasta : Enemy
{
    public override string Name => "COPYPASTA";
    public override Vector2 InfoBoxOffset => new(0, -115);
    public override Vector2 DamageNumberOffset => new(0, 50);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/copypasta.tres");
    protected override Stats Stats => new(420, 210, 42, 42, 42, 42, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["COPAttack", "COPDuplicate"];

    public override Task OnStartOfBattle()
    {
        ReinforcementSlots.ClaimPresetSlots(ReinforcementSlots.Copypasta);
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["COPAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                if (Roll() < 41)
                    goto attack;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 5)
                    goto attack;
                goto duplicate;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 5)
                    goto attack;
                goto duplicate;
            default:
                if (Roll() < 51)
                    goto attack;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 5)
                    goto attack;
                goto duplicate;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["COPAttack"]);
    duplicate:
        return new BattleCommand(this, this, Skills["COPDuplicate"]);
    }
}
