public class HumphreyUvula : Enemy
{
    public override string Name => "HUMPHREY";
    public override string AnimationPath => "res://animations/uvula.tres";
    protected override Stats Stats => new(1, 1, 1, 1, 900, 1, 95);
    protected override string[] EquippedSkills => [];

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "toast";
    }

    public override bool FallsOffScreen => false;

    private string[] Text = [
        "Heh! You'll never defeat me, loser!",
        "You are nothing but meat, so please let me eat you.",
        "Do not attack me, foodstuff. It is ineffectual!",
        "This is why I shouldn't play with my food.",
        "Yum, yum, yum...! It's time for grub!",
        "The stronger prey on the weaker! Don't you know?! That's biology!",
        "Please wait to be digested! There is a queue, so you'll have to wait.",
        "Do you see anyone else struggling?\nBehave yourselves, foodstuff!"
    ];

    public override BattleCommand ProcessAI()
    {
        // TODO: pulsate
        int roll = GameManager.Instance.Random.RandiRange(0, Text.Length - 1);
        BattleLogManager.Instance.QueueMessage(Text[roll]);
        return new BattleCommand(this, null, null);
    }
}