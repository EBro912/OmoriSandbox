using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class FishBunny : Enemy
{
    public override string Name => "FISH BUNNY";
    public override Vector2 InfoBoxOffset => new(0, -150);
    public override Vector2 DamageNumberOffset => new(0, 40);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/fish_bunny.tres");
    protected override Stats Stats => new(280, 90, 40, 13, 42, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["FSBAttack", "FSBDoNothing", "FSBSchooling"];

    public override Task OnStartOfBattle()
    {
        ReinforcementSlots.ClaimPresetSlots(ReinforcementSlots.FishBunny);
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["FSBAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto attack;
                goto schooling;
            case "sad":
                if (Roll() < 31)
                    goto attack;
                if (Roll() < 46)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto doNothing;
                goto schooling;
            case "angry":
                if (Roll() < 51)
                    goto attack;
                if (Roll() < 16)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto doNothing;
                goto schooling;
            default:
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto attack;
                goto schooling;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["FSBAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["FSBDoNothing"]);
    schooling:
        return new BattleCommand(this, this, Skills["FSBSchooling"]);
    }
}
