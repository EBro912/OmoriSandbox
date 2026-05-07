using Godot;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Editor;

internal partial class StatAdjustmentEditor : Control
{
    [Export] private OptionButton PresetDropdown;
    [Export] private Button AddButton;
    [Export] private Button ResetButton;
    [Export] private SpinBox HeartBox;
    [Export] private SpinBox JuiceBox;
    [Export] private SpinBox ATKBox;
    [Export] private SpinBox DEFBox;
    [Export] private SpinBox SPDBox;
    [Export] private SpinBox LCKBox;
    [Export] private SpinBox HITBox;
    [Export] private Label HeartLabel;
    [Export] private Label JuiceLabel;
    [Export] private Label ATKLabel;
    [Export] private Label DEFLabel;
    [Export] private Label SPDLabel;
    [Export] private Label LCKLabel;
    [Export] private Label HITLabel;
    
    [Signal]
    public delegate void StatsAdjustedEventHandler();
    
    public override void _Ready()
    {
        AddButton.Pressed += ApplyPreset;
        ResetButton.Pressed += Reset;
        HeartBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
        JuiceBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
        ATKBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
        DEFBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
        SPDBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
        LCKBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
        HITBox.ValueChanged += _ => EmitSignal(SignalName.StatsAdjusted);
    }

    public Stats GetStats()
    {
        return new Stats
        {
            HP = (int)HeartBox.Value,
            Juice = (int)JuiceBox.Value,
            ATK = (int)ATKBox.Value,
            DEF = (int)DEFBox.Value,
            SPD = (int)SPDBox.Value,
            LCK = (int)LCKBox.Value,
            HIT = (int)HITBox.Value
        };
    }

    public void SetStats(Stats stats)
    {
        HeartBox.SetValueNoSignal(stats.HP);
        JuiceBox.SetValueNoSignal(stats.Juice);
        ATKBox.SetValueNoSignal(stats.ATK);
        DEFBox.SetValueNoSignal(stats.DEF);
        SPDBox.SetValueNoSignal(stats.SPD);
        LCKBox.SetValueNoSignal(stats.LCK);
        HITBox.SetValueNoSignal(stats.HIT);
    }

    public void UpdateStats(Stats stats)
    {
        HeartLabel.Text = "Heart: " + stats.MaxHP;
        JuiceLabel.Text = "Juice: " + stats.MaxJuice;
        ATKLabel.Text = "ATK: " + stats.ATK;
        DEFLabel.Text = "DEF: " + stats.DEF;
        SPDLabel.Text = "SPD: " + stats.SPD;
        LCKLabel.Text = "LCK: " + stats.LCK;
        HITLabel.Text = "HIT: " + stats.HIT;
    }

    private void Reset()
    {
        HeartBox.SetValueNoSignal(0);
        JuiceBox.SetValueNoSignal(0);
        ATKBox.SetValueNoSignal(0);
        DEFBox.SetValueNoSignal(0);
        SPDBox.SetValueNoSignal(0);
        LCKBox.SetValueNoSignal(0);
        HITBox.SetValueNoSignal(0);
        EmitSignal(SignalName.StatsAdjusted);
    }
    
    private void ApplyPreset()
    {
        switch (PresetDropdown.Selected)
        {
            case 0:
                HeartBox.SetValueNoSignal(HeartBox.Value + 50);
                JuiceBox.SetValueNoSignal(JuiceBox.Value + 50);
                ATKBox.SetValueNoSignal(ATKBox.Value + 10);
                DEFBox.SetValueNoSignal(DEFBox.Value + 10);
                SPDBox.SetValueNoSignal(SPDBox.Value + 10);
                break;
            case 1:
                JuiceBox.SetValueNoSignal(JuiceBox.Value + 50);
                break;
            case 2:
                HeartBox.SetValueNoSignal(HeartBox.Value + 50);
                break;
            case 3:
                ATKBox.SetValueNoSignal(ATKBox.Value + 20);
                break;
            case 4:
                HeartBox.SetValueNoSignal(HeartBox.Value + 1);
                JuiceBox.SetValueNoSignal(JuiceBox.Value + 1);
                ATKBox.SetValueNoSignal(ATKBox.Value + 1);
                DEFBox.SetValueNoSignal(DEFBox.Value + 1);
                SPDBox.SetValueNoSignal(SPDBox.Value + 1);
                LCKBox.SetValueNoSignal(LCKBox.Value + 1);
                break;
            case 5:
                HeartBox.SetValueNoSignal(HeartBox.Value + 10);
                JuiceBox.SetValueNoSignal(JuiceBox.Value + 10);
                ATKBox.SetValueNoSignal(ATKBox.Value + 10);
                DEFBox.SetValueNoSignal(DEFBox.Value + 10);
                SPDBox.SetValueNoSignal(SPDBox.Value + 10);
                LCKBox.SetValueNoSignal(LCKBox.Value + 10);
                break;
            case 6:
                HeartBox.SetValueNoSignal(HeartBox.Value + 1);
                break;
            case 7:
                JuiceBox.SetValueNoSignal(JuiceBox.Value + 5);
                break;
        }
        EmitSignal(SignalName.StatsAdjusted);
    }
}