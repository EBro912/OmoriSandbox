using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStream> SFXDictionary = [];

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

		Instance = this;
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
}
