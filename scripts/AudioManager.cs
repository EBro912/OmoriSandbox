using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	[Export] private AudioStreamPlayer BGM;

	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStream> SFXDictionary = [];
	private readonly Dictionary<string, AudioStream> BGMDictionary = [];

	public static AudioManager Instance { get; private set; }

	public override void _Ready()
	{
		foreach (Node node in GetChildren())
		{
			AudioPlayers.Add((AudioStreamPlayer)node);
		}

		SFXDictionary.Add("Select", GD.Load<AudioStream>("res://audio/SYS_select.ogg"));
		SFXDictionary.Add("Cancel", GD.Load<AudioStream>("res://audio/sys_cancel.ogg"));
		SFXDictionary.Add("Buzzer", GD.Load<AudioStream>("res://audio/sys_buzzer.ogg"));
		SFXDictionary.Add("Move", GD.Load<AudioStream>("res://audio/SYS_move.ogg"));

		BGMDictionary.Add("BattleVF", GD.Load<AudioStream>("res://audio/battle_vf.ogg"));
		BGMDictionary.Add("Invitation", GD.Load<AudioStream>("res://audio/invitation.ogg"));
		BGMDictionary.Add("BossSlimeGirls", GD.Load<AudioStream>("res://audio/boss_slimegirls.ogg"));
		BGMDictionary.Add("Victory", GD.Load<AudioStream>("res://audio/xx_victory.ogg"));

		Instance = this;

		PlayBGM("Invitation");
	}

	public void PlaySFX(string name)
	{
		if (!SFXDictionary.TryGetValue(name, out AudioStream stream)) {
			GD.PrintErr("Unknown SFX: " + name);
			return;
		}

		foreach (AudioStreamPlayer player in AudioPlayers)
		{
			if (player.Playing)
				continue;
			player.Stream = stream;
			player.Play();
			break;
		}
	}

	public void PlayBGM(string name)
	{
		if (!BGMDictionary.TryGetValue(name, out AudioStream stream))
		{
			GD.PrintErr("Unknown SFX: " + name);
			return;
		}

		BGM.Stream = stream;
		BGM.Play();
	}
}
