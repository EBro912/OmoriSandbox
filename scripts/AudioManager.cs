using System;
using Godot;
using OmoriSandbox.Animation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OmoriSandbox;

/// <summary>
/// Handles all audio playback, including SFX and BGM.
/// </summary>
public partial class AudioManager : Node
{
	[Export] internal AudioStreamPlayer BGM { get; private set; }

	private readonly List<AudioStreamPlayer> AudioPlayers = [];

	private readonly Dictionary<string, AudioStreamOggVorbis> SFXDictionary = [];
	private readonly SortedDictionary<string, AudioStreamOggVorbis> BGMDictionary = [];

	public static AudioManager Instance { get; private set; }

	// only one instance of a sound can play at once
	private Dictionary<string, AudioStreamPlayer> PlayingSounds = [];

	public override void _EnterTree()
	{
		Instance = this;
	}

	internal void Init()
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
		int failedPreloads = 0;
		foreach (RPGMAnimatedSprite animation in AnimationManager.Instance.GetAllAnimations())
		{
			foreach (List<SFX> sfxList in animation.AllSFX)
			{
				foreach (SFX sfx in sfxList)
				{
					// we're technically checking if the file exists twice here,
					// but doing so avoids console spam
					if (ResourceLoader.Exists("res://audio/sfx/" + sfx.Name + ".ogg"))
					{
						SFXDictionary.TryAdd(sfx.Name, ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/sfx/" + sfx.Name + ".ogg"));
					}
					else
					{
						failedPreloads++;
					}
				}
			}
		}
		GD.Print($"Preloaded {SFXDictionary.Count} animation SFX. ({failedPreloads} failures)");
		
		// preload bgm
		foreach (string bgm in ResourceLoader.ListDirectory("res://audio/bgm"))
		{
			AudioStreamOggVorbis stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/bgm/" + bgm);
			stream.Loop = true;
			stream.LoopOffset = 0;
			BGMDictionary.Add(bgm.GetBaseName(), stream);
		}

		GD.Print($"Preloaded {BGMDictionary.Count} vanilla BGM.");
	}

	internal void PlaySFX(SFX sfx)
	{
		PlaySFX(sfx.Name, sfx.Pitch / 100f, sfx.Volume / 100f);
	}

	/// <summary>
	/// Plays SFX with the given <paramref name="name"/>. 
	/// </summary>
	/// <remarks>
	/// If an SFX of the same <paramref name="name"/> is already playing, it will be restarted.
	/// </remarks>
	/// <param name="name">The name of the SFX to play.</param>
	/// <param name="pitch">The pitch to play the SFX at.</param>
	/// <param name="volume">The volume to play the SFX at.</param>
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

		if (PlayingSounds.TryGetValue(stream.ResourcePath, out AudioStreamPlayer existing))
		{
			existing.Stream = stream;
			existing.PitchScale = pitch;
			existing.VolumeLinear = volume;
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
			PlayingSounds.Add(stream.ResourcePath, player);
			return;
		}

		GD.Print("Overloaded! We ran out of AudioStreams!");
	}

	/// <summary>
	/// Plays BGM with the given <paramref name="name"/>.
	/// </summary>
	/// <param name="name">The name of the BGM to play.</param>
	public void PlayBGM(string name)
	{
		PlayBGM(name, 1f, 1f);
	}

	/// <summary>
	/// Plays BGM with the given <paramref name="name"/> and desired parameters.
	/// </summary>
	/// <param name="name">The name of the BGM to play.</param>
	/// <param name="volume">The volume to play the BGM at, from 0.001 to 2.0.</param>
	/// <param name="pitch">The pitch to play the BGM at, from 0.1 to 2.0.</param>
	public void PlayBGM(string name, float volume, float pitch)
	{
		if (!TryGetBGM(name, out AudioStreamOggVorbis stream))
		{
			GD.PrintErr("Unknown BGM: " + name);
			return;
		}
		
		// prevent people from blowing out their eardrums
		volume = Math.Clamp(volume, 0.001f, 2f);
		pitch = Math.Clamp(pitch, 0.1f, 2f);
		
		BGM.Stream = stream;
		BGM.PitchScale = pitch;
		BGM.VolumeDb = Mathf.LinearToDb(volume);
		BGM.Play();
	}

	public bool TryGetBGM(string name, out AudioStreamOggVorbis stream)
	{
		if (BGMDictionary.TryGetValue(name, out stream))
			return true;
		
		if (ResourceLoader.Exists("res://audio/bgm/" + name + ".ogg"))
		{
			stream = ResourceLoader.Load<AudioStreamOggVorbis>("res://audio/bgm/" + name + ".ogg");
			stream.Loop = true;
			BGMDictionary.Add(name, stream);
			return true;
		}

		return false;
	}

	/// <summary>
	/// Stops the currently playing BGM.
	/// </summary>
	public void StopBGM()
	{
		BGM.Stop();
	}

	/// <summary>
	/// Pauses the currently playing BGM.
	/// </summary>
	public void PauseBGM(bool paused)
	{
		BGM.StreamPaused = paused;
	}

	/// <summary>
	/// Seeks the BGM to the given time in seconds.
	/// </summary>
	/// <param name="seconds">The time to seek to, in seconds.</param>
	public void SeekBGM(float seconds)
	{
		BGM.Seek(seconds);
	}

	/// <summary>
	/// Sets the loop offset for the BGM.
	/// </summary>
	/// <param name="seconds">The time offset to loop to, in seconds. Values greater than the length of the track will be ignored.</param>
	public void SetBGMLoopOffset(double seconds)
	{
		if (BGM.Stream is AudioStreamOggVorbis stream)
		{
			if (seconds < stream.GetLength())
				stream.LoopOffset = seconds;
		}
	}

	internal float GetBGMPosition()
	{
		return BGM.GetPlaybackPosition() + (float)AudioServer.GetTimeSinceLastMix();
	}

	internal bool LoadCustomBGM(string path)
	{
		AudioStreamOggVorbis stream = AudioStreamOggVorbis.LoadFromFile(path);
		stream.Loop = true;
		string name = path.GetFile().GetBaseName();
		return BGMDictionary.ContainsKey(name) || BGMDictionary.TryAdd(path.GetFile().GetBaseName(), stream);
	}

	internal bool LoadCustomSFX(string path)
	{
		AudioStreamOggVorbis stream = AudioStreamOggVorbis.LoadFromFile(path);
		string name = path.GetFile().GetBaseName();
		return SFXDictionary.ContainsKey(name) || SFXDictionary.TryAdd(path.GetFile().GetBaseName(), stream);
	}

	/// <summary>
	/// Fades the BGM to the given <paramref name="volume"/> over the given number of <paramref name="seconds"/>.
	/// </summary>
	/// <param name="volume">The volume to fade the BGM to, from 0.001 to 2.0.</param>
	/// <param name="seconds">How long it should take for the BGM to fade, in seconds.</param>
	public void FadeBGMTo(float volume, float seconds = 1f)
	{
		volume = Math.Clamp(volume, 0.001f, 2f);
		float target = Mathf.LinearToDb(volume);
		if (seconds == 0f)
			BGM.VolumeDb = target;
		else
		{
			Tween tween = CreateTween();
			tween.TweenProperty(BGM, "volume_db", target, seconds);
		}
	}

	/// <summary>
	/// Fades the BGM to the given <paramref name="volume"/> over the given number of <paramref name="seconds"/> and waits for it to finish.
	/// </summary>
	/// <param name="volume">The volume to fade the BGM to, from 0.001 to 2.0.</param>
	/// <param name="seconds">How long it should take for the BGM to fade, in seconds.</param>
	public async Task WaitForFadeBGMTo(float volume, float seconds = 1f)
	{
		volume = Math.Clamp(volume, 0.001f, 2f);
		float target = Mathf.LinearToDb(volume);
		if (seconds == 0f)
			BGM.VolumeDb = target;
		else
		{
			Tween tween = CreateTween();
			tween.TweenProperty(BGM, "volume_db", target, seconds);
			await ToSignal(tween, Tween.SignalName.Finished);
		}
	}

	private void OnSFXFinish(AudioStreamPlayer player)
	{
		player.PitchScale = 1f;
		player.VolumeLinear = 1f;
		PlayingSounds.Remove(player.Stream.ResourcePath);
	}

	internal IEnumerable<string> GetAllBGM()
	{
		return BGMDictionary.Keys;
	}

	internal void Reset()
	{
		PlayingSounds.Clear();
		BGM.Stop();
		BGM.PitchScale = 1f;
		BGM.VolumeDb = 1f;
		foreach (AudioStreamPlayer player in AudioPlayers)
		{
			player.Stop();
			player.Stream = null;
			player.PitchScale = 1f;
			player.VolumeLinear = 1f;
		}
	}
}
