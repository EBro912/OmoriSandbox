using System.Collections.Generic;
using Godot;
using OmoriSandbox.Editor;

namespace OmoriSandbox;

internal partial class BossRushStageEditorComponent : Control
{
    [Export] public BattlebackBGMEditorComponent BattlebackBGMEditor { get; private set; }
    [Export] public CheckBox HealPartyCheckbox { get; private set; }
    [Export] public CheckBox KeepEmotionCheckbox { get; private set; }
    [Export] public CheckBox KeepStatusEffectsCheckbox { get; private set; }
    [Export] private Button AddEnemyButton;
    [Export] public TabContainer Enemies { get; private set; }
    [Export] private PackedScene EnemyEditor;
    [Export] public Node EnemyParent { get; private set; }

    public override void _Ready()
    {
        AddEnemyButton.Pressed += () =>
        {
            AnimatedSprite2D enemySprite = new();
            EnemyParent.AddChild(enemySprite);
            EnemyEditorComponent editor = EnemyEditor.Instantiate<EnemyEditorComponent>();
            Enemies.AddChild(editor);
            editor.Init(enemySprite);
        };
    }

    public void CopyFrom(BattlePreset source)
    {
        if (source.Type is GameModeType.BossRush)
            return;

        BattlebackBGMEditor.SelectedBattleback = source.Battleback;
        BattlebackBGMEditor.SelectedBGM = source.BGM;
        BattlebackBGMEditor.BGMLoopPointValue = source.BGMLoopPoint;
        BattlebackBGMEditor.BGMPitchValue = source.BGMPitch;
        foreach (BattlePresetEnemy enemy in source.Enemies)
        {
            AnimatedSprite2D enemySprite = new();
            EnemyParent.AddChild(enemySprite);
            enemySprite.Visible = false;
            EnemyEditorComponent editor = EnemyEditor.Instantiate<EnemyEditorComponent>();
            Enemies.AddChild(editor);
            editor.Init(enemySprite, enemy);
        }
    }
    
    public void CopyFrom(BossRushStageEditorComponent source)
    {
        BattlebackBGMEditor.SelectedBattleback = source.BattlebackBGMEditor.SelectedBattleback;
        BattlebackBGMEditor.SelectedBGM = source.BattlebackBGMEditor.SelectedBGM;
        BattlebackBGMEditor.BGMPitchValue = source.BattlebackBGMEditor.BGMPitchValue;
        BattlebackBGMEditor.BGMLoopPointValue = source.BattlebackBGMEditor.BGMLoopPointValue;
        HealPartyCheckbox.ButtonPressed = source.HealPartyCheckbox.ButtonPressed;
        KeepEmotionCheckbox.ButtonPressed = source.KeepEmotionCheckbox.ButtonPressed;
        KeepStatusEffectsCheckbox.ButtonPressed = source.KeepStatusEffectsCheckbox.ButtonPressed;
        foreach (Node node in source.Enemies.GetChildren())
        {
            if (node is EnemyEditorComponent enemy)
            {
                AnimatedSprite2D enemySprite = new();
                EnemyParent.AddChild(enemySprite);
                enemySprite.Visible = false;
                EnemyEditorComponent editor = EnemyEditor.Instantiate<EnemyEditorComponent>();
                Enemies.AddChild(editor);
                editor.Init(enemySprite, enemy.EnemyDropdown.GetItemText(enemy.EnemyDropdown.Selected), 
                    new Vector2((float)enemy.XPosBox.Value, (float)enemy.YPosBox.Value), enemy.EmotionDropdown.GetItemText(enemy.EmotionDropdown.Selected), 
                    (int)enemy.LayerBox.Value, enemy.FallsOffScreenCheckbox.ButtonPressed, enemy.GrayscaleOnDefeatCheckbox.ButtonPressed);
            }
        }
    }

    public void HideEnemies()
    {
        foreach (Node child in EnemyParent.GetChildren())
        {
            if (child is AnimatedSprite2D sprite)
                sprite.Visible = false;
        }
    }

    public void ShowEnemies()
    {
        foreach (Node child in EnemyParent.GetChildren())
        {
            if (child is AnimatedSprite2D sprite)
                sprite.Visible = true;
        }
    }
    
}