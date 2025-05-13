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

		DirAccess sfx = DirAccess.Open("res://audio/sfx");
		foreach (string f in sfx.GetFiles())
		{
			if (f.Contains(".import")) continue;
			string name = f.Split('.')[0];
			SFXDictionary.Add(name, GD.Load<AudioStream>("res://audio/sfx/" + f));
		}
		GD.Print("Loaded " + SFXDictionary.Count + " SFX.");

		DirAccess bgm = DirAccess.Open("res://audio/bgm");
		foreach (string f in bgm.GetFiles())
		{
			if (f.Contains(".import")) continue;
			string name = f.Split('.')[0];
			BGMDictionary.Add(name, GD.Load<AudioStream>("res://audio/bgm/" + f));
		}
		GD.Print("Loaded " + BGMDictionary.Count + " BGM.");
		
		Instance = this;

		PlayBGM("invitation");
		BGM.Finished += OnBGMFinish;
	}

	public void PlaySFX(string name, float pitch = 1f)
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
			player.PitchScale = pitch;
			// TODO: Volume
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

	private void OnBGMFinish()
	{
		BGM.Play();
	}
}
