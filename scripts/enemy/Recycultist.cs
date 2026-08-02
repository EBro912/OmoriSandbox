using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class Recycultist : Enemy
{
    public override string Name => "RECYCULTIST";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>($"res://animations/recycultist_{(FacingLeft ? "left" : "right")}.tres");

    // instead of making an entire other enemy for the right facing animation
    // just use a constructor value to dictate the direction it faces
    private readonly bool FacingLeft;
    
    public Recycultist(bool facingLeft)
    {
        FacingLeft = facingLeft;
    }

    protected override Stats Stats => new(130, 65, 30, 20, 13, 10, 95);

    protected override string[] EquippedSkills => ["RCultGatherTrash", "RCultFlingTrash"];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id == "neutral" || emotion.Id == "sad" || emotion.Id == "happy" || emotion.Id == "angry";
    }

    public override BattleCommand ProcessAI()
    {
        if (Roll() < 51)
            return new BattleCommand(this, SelectTarget(), Skills["RCultFlingTrash"]);
        return new BattleCommand(this, this, Skills["RCultGatherTrash"]);
    }
}