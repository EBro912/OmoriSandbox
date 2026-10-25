using System.Linq;
using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Animation;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Mussel : Enemy
{
    public override string Name => "MUSSEL";
    public override Vector2 InfoBoxOffset => new(0, -200);
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/mussel.tres");
    protected override Stats Stats => new(100, 100, 55, 85, 1, 10, 95);

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    protected override string[] EquippedSkills => ["MUSAttack", "MUSFlex", "MUSHide"];

    public override BattleCommand ProcessAI()
    {
        if (HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["MUSAttack"]);

        switch (CurrentEmotion.Group?.Id)
        {
            case "happy":
                if (Roll() < 41)
                    goto attack;
                if (Roll() < 56)
                    goto flex;
                goto hide;
            case "sad":
                if (Roll() < 26)
                    goto attack;
                if (Roll() < 26)
                    goto flex;
                goto hide;
            case "angry":
                if (Roll() < 71)
                    goto attack;
                if (Roll() < 41)
                    goto flex;
                goto hide;
            default:
                if (Roll() < 56)
                    goto attack;
                if (Roll() < 36)
                    goto flex;
                goto hide;
        }
    attack:
        return new BattleCommand(this, SelectTarget(), Skills["MUSAttack"]);
    flex:
        return new BattleCommand(this, this, Skills["MUSFlex"]);
    hide:
        return new BattleCommand(this, this, Skills["MUSHide"]);
    }

    private bool HasFeltInferior;

    public override async Task ProcessEndOfTurn()
    {
        if (HasFeltInferior || BattleManager.Instance.TurnCount < 1)
            return;
        PartyMember kel = BattleManager.Instance.GetPartyMemberAtPosition(2);
        if (kel == null || !kel.HasStatModifier("Flex"))
            return;
        Mussel[] inferior = [.. BattleManager.Instance.GetAllEnemies().OfType<Mussel>().Where(x => !x.HasFeltInferior && !x.IsToast)];
        DialogueManager.Instance.QueueMessage("The MUSSEL sees KEL's FLEX and feels inferior...");
        await DialogueManager.Instance.WaitForDialogue();
        foreach (Mussel mussel in inferior)
        {
            mussel.HasFeltInferior = true;
            mussel.AddTierStatModifier("AttackDown", 3, silent: true);
            mussel.AddTierStatModifier("DefenseDown", 3, silent: true);
            mussel.AddTierStatModifier("SpeedDown", 3, silent: true);
            AnimationManager.Instance.PlayAnimation(219, mussel);
        }
    }
}
