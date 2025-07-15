using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	[Export] private AudioStreamPlayer BGM;

	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStreamOggVorbis> SFXDictionary = [];
	private readonly Dictionary<string, AudioStreamOggVorbis> BGMDictionary = [];

	public static AudioManager Instance { get; private set; }

	// only one instance of a sound can play at once
	private Dictionary<string, AudioStreamPlayer> PlayingSounds = [];

	public override void _EnterTree()
	{
		Instance = this;
	}

	public void Init()
	{
		if (SFXDictionary.Count > 0)
		{
			GD.PrintErr("Attempting to re-init AudioManager, ignoring!");
			return;
		}

		foreach (Node node in GetChildren())
		{
			AudioStreamPlayer player = node as AudioStreamPlayer;
			AudioPlayers.Add(player);
			player.Finished += () => OnSFXFinish(player);
		}
		// preload animation sfx
		foreach (RPGMAnimatedSprite animation in GameManager.Instance.AnimationManager.GetAllAnimations())
		{
			foreach (List<SFX> sfxList in animation.AllSFX)
			{
				foreach (SFX sfx in sfxList)
				{
					SFXDictionary.TryAdd(sfx.Name, ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/sfx/" + sfx.Name + ".ogg"));
				}
			}
		}
		GD.Print("Preloaded " + SFXDictionary.Count + " SFX.");
		// BGM.Finished += OnBGMFinish;
	}

	public void PlaySFX(SFX sfx)
	{
		PlaySFX(sfx.Name, sfx.Pitch / 100f, sfx.Volume / 100f);
	}

	public void PlaySFX(string name, float pitch = 1f, float volume = 1f)
	{
		if (!SFXDictionary.TryGetValue(name, out AudioStreamOggVorbis stream)) {
			stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/sfx/" + name + ".ogg");
			if (stream == null)
			{
				GD.PrintErr("Unknown SFX: " + name);
				return;
			}
			SFXDictionary.Add(name, stream);
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
		if (!BGMDictionary.TryGetValue(name, out AudioStreamOggVorbis stream))
		{
			if (ResourceLoader.Exists("res://audio/bgm/" + name + ".ogg"))
				stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/bgm/" + name + ".ogg");
			// check the custom folder too
			else if (FileAccess.FileExists(GameManager.Instance.CustomDataPath + "/bgm/" + name + ".ogg"))
				stream = LoadCustomBGM(GameManager.Instance.CustomDataPath + "/bgm/" + name + ".ogg");
			else
			{
				GD.PrintErr("Unknown BGM: " + name);
				return;
			}
            stream.Loop = true;
            // TOOD: allow custom BGM to have a loop offset
            stream.LoopOffset = 0d;
            BGMDictionary.Add(name, stream);
		}

		BGM.Stream = stream;
		BGM.Play();
	}

	private AudioStreamOggVorbis LoadCustomBGM(string path)
	{
		AudioStreamOggVorbis stream = AudioStreamOggVorbis.LoadFromFile(path);
		return stream;
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
