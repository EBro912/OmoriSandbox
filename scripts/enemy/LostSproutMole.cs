public class LostSproutMole : Enemy
{
    public override string Name => "LOST SPROUT MOLE";

    public override string AnimationPath => "res://animations/sprout_mole.tres";

    protected override Stats Stats => new(62, 31, 11, 8, 9, 5, 95);

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }

    public override BattleCommand ProcessAI()
    {
        int roll = GameManager.Instance.Random.RandiRange(0, 100);
        if (roll < 36)
        {
            PartyMember target = GameManager.Instance.BattleManager.GetRandomAlivePartyMember();
            return new AttackCommand(this, target, "[actor] bumps into [target]!");
        }
        return new DoNothingCommand(this, "[actor] is rolling around.");
    }
}
