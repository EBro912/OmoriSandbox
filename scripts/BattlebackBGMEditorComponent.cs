using System;
using Godot;
using OmoriSandbox.Extensions;
using Range = Godot.Range;

namespace OmoriSandbox.Editor;

internal partial class BattlebackBGMEditorComponent : Control
{
    public override void _Process(double delta)
    {
        if (PreviewingBGM && !BGMPlayer.Editable)
        {
            BGMPlayer.Value = BGMPreview.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
        }
    }

    public void Init(AudioStreamPlayer bgmPreview, BattlebackDisplayComponent battlebackPreview)
    {
	    BGMPreview = bgmPreview;
	    BattlebackPreview = battlebackPreview;
	    
        foreach (string battleback in BattlebackManager.Instance.GetAllBattlebacks())
        {
            BattlebackDropdown.AddItem(battleback);
        }

        foreach (string bgm in AudioManager.Instance.GetAllBGM())
        {
            BGMDropdown.AddItem(bgm);
        }
        
        BGMDropdown.Selected = BGMDropdown.GetItemIndex("battle_vf");
        if (!BGMPreview.Playing && AudioManager.Instance.TryGetBGM("battle_vf", out AudioStreamOggVorbis stream))
        {
            BGMPreview.Stream = stream;
            BGMPlayer.MaxValue = stream.GetLength();
            LoopPoint.MaxValue = stream.GetLength();
            BGMPlayButton.Text = "Play";
            BGMPlayer.Editable = true;
            BGMPreview.Play();
            BGMPreview.StreamPaused = true;
        }
        
        BattlebackDropdown.Selected = BattlebackDropdown.GetItemIndex("battleback_vf_default");
		BattlebackDropdown.ItemSelected += (idx) =>
		{
			string battleback = BattlebackDropdown.GetItemText((int)idx);
			BattlebackPreview.SetBattleback(battleback);	
		};
				
		BGMDropdown.ItemSelected += (idx) =>
		{
			BGMPreview.Stop();
			BGMPlayerTimer.Text = "00:00.00";
			BGMPlayer.Value = 0;
			BGMPlayer.MaxValue = 0;
			BGMPlayer.Editable = true;
			BGMPlayButton.Text = "Play";
			BGMPitch.Value = 1;
			LoopPoint.Value = 0;
			LoopPoint.MaxValue = 0;
			PreviewingBGM = false;
			string bgm = BGMDropdown.GetItemText((int)idx);
			if (AudioManager.Instance.TryGetBGM(bgm, out AudioStreamOggVorbis s))
			{
				BGMPreview.Stream = s;
				BGMPlayer.MaxValue = s.GetLength();
				LoopPoint.MaxValue = s.GetLength();
				BGMPlayButton.Text = "Play";
				BGMPreview.Play();
				BGMPreview.StreamPaused = true;
			}
		};
		
		BGMPlayButton.Pressed += () =>
		{
			if (BGMDropdown.Selected == -1)
				return;

			if (BGMPlayButton.Text == "Play")
			{
				BGMPlayButton.Text = "Pause";
				BGMPreview.StreamPaused = false;
				BGMPreview.Seek((float)BGMPlayer.Value);
				BGMPlayer.Editable = false;
				PreviewingBGM = true;
			}
			else
			{
				BGMPlayButton.Text = "Play";
				BGMPreview.StreamPaused = true;
				BGMPlayer.Editable = true;
				PreviewingBGM = false;
			}
		};
		
		LoopSetCurrentButton.Pressed += () =>
		{
			LoopPoint.Value = BGMPlayer.Value;
		};

		LoopPoint.ValueChanged += (value) =>
		{
			if (BGMPreview.Stream is AudioStreamOggVorbis s)
			{
				if (value < s.GetLength())
					s.LoopOffset = value;
			}
		};

		BGMPlayer.ValueChanged += (value) =>
		{
			BGMPlayerTimer.Text = TimeSpan.FromSeconds(value).ToString(@"mm\:ss\.ff");
		};

		BGMPitch.ValueChanged += (value) =>
		{
			BGMPreview.PitchScale = (float)value;
		};
    }

    public void Stop()
    {
	    if (PreviewingBGM)
	    {
		    BGMPlayButton.EmitSignal("pressed");
	    }
    }

    public void Load()
    {
	    BattlebackDropdown.EmitSignal("item_selected", BattlebackDropdown.Selected);
	    BattlebackPreview.SetBattleback(SelectedBattleback);
	    string bgm = BGMDropdown.GetItemText(BGMDropdown.Selected);
	    if (AudioManager.Instance.TryGetBGM(bgm, out AudioStreamOggVorbis s))
	    {
		    BGMPreview.Stream = s;
		    BGMPlayer.MaxValue = s.GetLength();
		    LoopPoint.MaxValue = s.GetLength();
		    BGMPreview.PitchScale = (float)BGMPitch.Value;
		    BGMPlayButton.Text = "Play";
		    BGMPreview.Play();
		    BGMPreview.Seek((float)BGMPlayer.Value);
		    BGMPreview.StreamPaused = true;
	    }
    }

    public void Reset()
    {
	    SelectedBattleback = "battleback_vf_default";
	    SelectedBGM = "battle_vf";
    }
    
    public string SelectedBattleback
    {
	    get => BattlebackDropdown.GetItemText(BattlebackDropdown.Selected);
	    set
	    {
		    int index = BattlebackDropdown.GetItemIndex(value);
		    if (index == -1)
		    {
			    GD.PushWarning($"Failed to load battleback {value}, falling back to default.");
			    value = "battleback_vf_default";
			    index = BattlebackDropdown.GetItemIndex(value);
		    }
		    BattlebackDropdown.Selected = index;
	    }
    }

    public string SelectedBGM
    {
	    get => BGMDropdown.GetItemText(BGMDropdown.Selected);
	    set
	    {
		    BGMDropdown.Selected = BGMDropdown.GetItemIndex(value);
		    if (BGMDropdown.Selected == -1)
		    {
			    GD.PushWarning($"Failed to load BGM {value}, falling back to default.");
			    value = "battle_vf";
			    BGMDropdown.Selected = BGMDropdown.GetItemIndex(value);
		    }

		    BGMDropdown.EmitSignal(OptionButton.SignalName.ItemSelected, [BGMDropdown.Selected]);
	    }
    }

    public double BGMPitchValue
    {
	    get => BGMPitch.Value;
	    set => BGMPitch.Value = Math.Clamp(value, 0.1d, 2.0d);
    }
    public double BGMLoopPointValue 
    {
	    get => LoopPoint.Value;
	    set => LoopPoint.Value = value;
    }

    private bool PreviewingBGM = false;
    private AudioStreamPlayer BGMPreview;
    private BattlebackDisplayComponent BattlebackPreview;
    [Export] private OptionButton BattlebackDropdown;
    [Export] private OptionButton BGMDropdown;
    [Export] private SpinBox BGMPitch;
    [Export] private Button BGMPlayButton;
    [Export] private Button LoopSetCurrentButton;
    [Export] private SpinBox LoopPoint;
    [Export] private Label BGMPlayerTimer;
    [Export] private HSlider BGMPlayer;
}