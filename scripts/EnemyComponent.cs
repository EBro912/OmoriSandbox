using Godot;

public partial class EnemyComponent : Node
{
    private Enemy Enemy;
    private Control AboveHead;
    private TextureProgressBar HPBar;
    private Label NameLabel;

    public Enemy Actor => Enemy;

    public void SetEnemy(Enemy enemy, string initialState)
    {
        Enemy = enemy;
        AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("../Sprite");
        Enemy.Init(sprite, initialState);
        AboveHead = GetNode<Control>("../AboveHead");
        NameLabel = GetNode<Label>("../AboveHead/Infobox/Name");
        HPBar = GetNode<TextureProgressBar>("../AboveHead/Infobox/Health");
        HPBar.MaxValue = Enemy.BaseStats.HP;
        HPBar.Value = Enemy.CurrentHP;
        NameLabel.Text = Enemy.Name;
        AboveHead.Visible = false;

        Enemy.CenterPoint = GetParent<Node2D>().GlobalPosition;
    }

    public override void _Process(double delta)
    {
        HPBar.Value = Enemy.CurrentHP;
    }

    public void ShowInfoBox(bool show)
    {
        AboveHead.Visible = show;
    }

    public void Despawn()
    {
        GetParent().QueueFree();
    }
}