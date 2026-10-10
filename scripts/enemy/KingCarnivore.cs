using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class KingCarnivore : Enemy
{
    public override string Name => "KING CARNIVORE";
    // VANILLA BUG: KING CARNIVORE's emotion entries have StatusXOffset 0 instead of 25, so its info box shifts left while it feels an emotion
    public override Vector2 InfoBoxOffset => CurrentEmotion.Group == null ? new(25, -295) : new(0, -295);
    public override Vector2 DamageNumberOffset => new(0, -200);
    public override bool InfoBoxCursorAboveBox => true;
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/king_carnivore.tres");
    protected override Stats Stats => new(1900, 950, 65, 29, 53, 10, 95);
    protected override string[] EquippedSkills => ["UPCAttack", "UPCDoNothing", "UPCSweetGas"];
    
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "sad" or "happy" or "angry" or "afraid";
    }

    private readonly List<Enemy> Roots = [];
    
    public override Task OnStartOfBattle()
    {
        Roots.Add(BattleManager.Instance.SummonEnemy("Root", new Vector2(CenterPoint.X + 240, CenterPoint.Y + 70), layer: Layer));
        Roots.Add(BattleManager.Instance.SummonEnemy("Root", new Vector2(CenterPoint.X - 240, CenterPoint.Y + 70), layer: Layer));
        return Task.CompletedTask;
    }

    public override Task OnDefeat()
    {
        foreach (Enemy enemy in Roots)
            enemy.CurrentHP = 0;
        return Task.CompletedTask;
    }

    public override BattleCommand ProcessAI()
    {
        switch (CurrentEmotion.Id)
        { 
            case "happy":
                if (Roll() < 76)
                    goto attack;
                goto nothing;
            case "sad":
               if (Roll() < 46)
                    goto attack;
               if (Roll() < 56)
                    goto nothing;
               goto gas;
            case "angry":
                if (Roll() < 61)
                    goto attack;
                if (Roll() < 31)
                    goto nothing;
                goto gas;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 41)
                    goto nothing;
                goto gas;
        }
        attack:
        return new BattleCommand(this, SelectTarget(), Skills["UPCAttack"]);
        nothing:
        return new BattleCommand(this, this, Skills["UPCDoNothing"]);
        gas:
        return new BattleCommand(this, SelectAllTargets(), Skills["UPCSweetGas"]);
    }
}