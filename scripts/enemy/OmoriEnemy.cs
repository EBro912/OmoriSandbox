using System.Threading.Tasks;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class OmoriEnemy : Enemy
{
    public override string Name => "OMORI";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>($"res://animations/omori_enemy_{Phase}.tres");
    protected override Stats Stats => new(255, 0, 54, 52, 256, 10, 1000);
    protected override string[] EquippedSkills => [];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral";
    }

    public override BattleCommand ProcessAI()
    {
        throw new System.NotImplementedException();
    }

    private int Phase = 1;
    
    private void UpdateSprite()
    {
        // change sprite on phase change
        Sprite.SpriteFrames = Animation;
        Sprite.Animation = "neutral";
        Sprite.Play();
    }
}