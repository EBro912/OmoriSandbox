public class LostSproutMole : Enemy
{
    public override string Name => "LOST SPROUT MOLE";

    public override string AnimationPath => "res://animations/sprout_mole.tres";

    public override EnemyStats Stats => new(42, 21, 3, 8, 5, 5);

    public override bool IsStateValid(string state)
    {
        return state == "neutral" || state == "sad" || state == "happy" || state == "angry" || state == "hurt" || state == "toast";
    }

    public override void ProcessAI()
    {
        
    }
}
