using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Wormhole : Enemy
{
    public override string Name => "WORMHOLE";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override Vector2 DamageNumberOffset => new(0, 30);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/wormhole.tres");
    protected override Stats Stats => new(150, 75, 10, 9, 10, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["WHAttack", "WHDoNothing", "WHOpenWormhole"];

    public override Task OnStartOfBattle()
    {
        ReinforcementSlots.ClaimPresetSlots(ReinforcementSlots.Wormhole);
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["WHAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 46)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto doNothing;
                goto openWormhole;
            case "angry":
                if (Roll() < 76)
                    goto attack;
                if (Roll() < 26)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto doNothing;
                goto openWormhole;
            default:
                if (Roll() < 36)
                    goto attack;
                if (Roll() < 21)
                    goto doNothing;
                if (BattleManager.Instance.GetAllAliveEnemies().Count == 3)
                    goto doNothing;
                goto openWormhole;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["WHAttack"]);
    doNothing:
        return new BattleCommand(this, this, Skills["WHDoNothing"]);
    openWormhole:
        return new BattleCommand(this, this, Skills["WHOpenWormhole"]);
    }
}
