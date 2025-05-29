using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	[Export] private AudioStreamPlayer BGM;

	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStream> SFXDictionary = [];
	private readonly Dictionary<string, AudioStream> BGMDictionary = [];

	public static AudioManager Instance { get; private set; }

	// only one instance of a sound can play at once
	private Dictionary<string, AudioStreamPlayer> PlayingSounds = [];

	public override void _Ready()
	{
		foreach (Node node in GetChildren())
		{
			AudioStreamPlayer player = node as AudioStreamPlayer;
			AudioPlayers.Add(player);
			player.Finished += () => OnSFXFinish(player);
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

		PlayBGM("boss_sweetheart");
		// BGM.Finished += OnBGMFinish;
	}

	public void PlaySFX(SFX sfx)
	{
		PlaySFX(sfx.Name, sfx.Pitch / 100f, sfx.Volume / 100f);
	}

	public void PlaySFX(string name, float pitch = 1f, float volume = 1f)
	{
		if (!SFXDictionary.TryGetValue(name, out AudioStream stream)) {
			GD.PrintErr("Unknown SFX: " + name);
			return;
		}

		if (PlayingSounds.TryGetValue(name, out AudioStreamPlayer existing))
		{
			existing.Stream = stream;
			existing.Play();
			return;
		}

		foreach (AudioStreamPlayer player in AudioPlayers)
		{
			if (player.Playing)
				continue;
			player.Stream = stream;
			player.PitchScale = pitch;
			player.VolumeLinear = volume;
			player.Play();
			PlayingSounds.Add(name, player);
			return;
		}

		GD.PushWarning("Overloaded! We ran out of AudioStreams!");
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

	public void FadeBGMTo(float volume, float seconds = 1f)
	{
		float target = -10 + Mathf.LinearToDb(volume / 100f);
		Tween tween = CreateTween();
		tween.TweenProperty(BGM, "volume_db", target, seconds);
	}

	private void OnSFXFinish(AudioStreamPlayer player)
	{
		foreach (var pair in PlayingSounds)
		{
			if (pair.Value == player)
			{
				PlayingSounds.Remove(pair.Key);
				break;
			}
		}
	}

	private void OnBGMFinish()
	{
		BGM.Play();
	}
}
