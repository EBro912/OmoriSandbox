using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class AnimationManager : Node2D
{
	[Signal]
	public delegate void AnimationFinishedEventHandler();

	private TextureRect Battleback;
	private AnimatedSprite2D ReleaseEnergy;

	private Dictionary<int, RPGMAnimatedSprite> Animations = [];

	private const float FPS = 15f;
	private float FrameDuration = 1f / FPS;
	private float FrameTimer = 0f;
	private List<PlayingAnimation> PlayingAnimations = [];

	private float Shake = 0f;
	private float ShakePwr = 0f;
	private float ShakeSpd = 0f;
	private int ShakeDuration = 0;
	private float ShakeDirection = -1f;

	public override void _Ready()
	{
		Battleback = GetNode<TextureRect>("../../UI/Battleback");
		ReleaseEnergy = GetNode<AnimatedSprite2D>("../../UI/ReleaseEnergy");

		string data = FileAccess.GetFileAsString("res://animations/animations.json");
		List<AnimationInfo> animationData = JsonConvert.DeserializeObject<List<AnimationInfo>>(data);
		foreach (AnimationInfo info in animationData)
		{
			RPGMAnimatedSprite animation;

			if (string.IsNullOrWhiteSpace(info.Texture))
				continue;

            if (!string.IsNullOrWhiteSpace(info.AltTexture))
                animation = new(info.Id, info.Layer, GD.Load<Texture2D>($"res://assets/animations/{info.Texture}.png"), GD.Load<Texture2D>($"res://assets/animations/{info.AltTexture}.png"));
            else
                animation = new(info.Id, info.Layer, GD.Load<Texture2D>($"res://assets/animations/{info.Texture}.png"));

            foreach (float[][] frame in info.Frames)
			{
				List<Frame> frames = [];
				foreach (float[] f in frame)
				{
					frames.Add(new Frame((int)f[0], f[1], f[2], f[3], f[4], f[5] == 1, f[6]));
				}
				animation.CreateFrame(frames);
			}
			foreach (SFXInfo sfx in info.SFX)
			{
				animation.SetFrameSFX(sfx.Frame, new SFX(sfx.Name, sfx.Pitch, sfx.Volume));
			}
			foreach (ShakeInfo shake in info.Shake)
			{
				animation.SetFrameShake(shake.Frame, shake.Power, shake.Speed, shake.Duration);
			}
			if (!Animations.TryAdd(info.Id, animation))
			{
				GD.PrintErr("Unable to add animation ID " + info.Id + ", is there a duplicate?");
			}
		}
		GD.Print($"Loaded {Animations.Count} animations");
	}

	public override void _Process(double delta)
	{
		if (PlayingAnimations.Count == 0)
			return;

		FrameTimer += (float)delta;

		if (FrameTimer >= FrameDuration)
		{
			FrameTimer -= FrameDuration;
			NextFrame();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// physics process runs at 60 fps, just like screen shake
		if (ShakeDuration > 0 || Shake != 0f)
		{
			UpdateShake();
		}

		float x = 0f;
		x += (float)Math.Round(Shake) - 640f;
		Battleback.Position = new Vector2(x, 0);
	}

	private void NextFrame()
	{
		for (int i = PlayingAnimations.Count - 1; i >= 0; i--)
		{
			// returns true if we're out of frames
			if (PlayingAnimations[i].AdvanceFrame())
			{
				PlayingAnimations[i].QueueFree();
				PlayingAnimations.RemoveAt(i);
				if (PlayingAnimations.Count == 0)
				{
					FrameTimer = 0f;
					EmitSignal(SignalName.AnimationFinished);
					return;
				}
				continue;
			}
			if (PlayingAnimations[i].Animation.TryGetFrameSFX(PlayingAnimations[i].CurrentFrame, out List<SFX> sfx))
			{
				sfx.ForEach(AudioManager.Instance.PlaySFX);
			}
			if (PlayingAnimations[i].Animation.TryGetFrameShake(PlayingAnimations[i].CurrentFrame, out Shake shake))
			{
				InitShake(shake);
			}
		}
	}

	private void UpdateShake()
	{
		float delta = (ShakePwr * (2f * ShakeSpd) * ShakeDirection) / 5f;
		if (ShakeDuration <= 1 && Shake * (Shake + delta) < 0)
			Shake = 0;
		else
			Shake += delta;
		if (Shake > ShakePwr * 2f)
			ShakeDirection = -1;
		if (Shake < -ShakePwr * 2f)
			ShakeDirection = 1;
		ShakePwr *= 0.9f;
		ShakeDuration--;
	}

	private void InitShake(Shake shake)
	{
		Battleback.Position = new Vector2(-640, 0);
		Shake = 0f;
		ShakePwr = shake.Power;
		ShakeSpd = shake.Speed;
		ShakeDuration = shake.Duration;
	}

	private void ResetShake()
	{
		Battleback.Position = new Vector2(-640, 0);
		Shake = 0f;
		ShakePwr = 0f;
		ShakeSpd = 0f;
		ShakeDuration = 0;
	}

	public void PlayAnimation(int id, Actor target, bool targetsEnemy = true)
	{
		StartAnimation(id, target.CenterPoint, targetsEnemy);
	}

	public void PlayScreenAnimation(int id, bool targetsEnemy)
	{
		StartAnimation(id, new Vector2(320, 240), targetsEnemy);
	}

	public Task WaitForAnimation(int id, Actor target, bool targetsEnemy = true)
	{
		TaskCompletionSource tcs = new();

		void Handle()
		{
			AnimationFinished -= Handle;
			tcs.SetResult();
		}	

		PlayAnimation(id, target, targetsEnemy);
		AnimationFinished += Handle;
		return tcs.Task;
	}

	public Task WaitForScreenAnimation(int id, bool targetsEnemy)
	{
		TaskCompletionSource tcs = new();

		void Handle()
		{
			AnimationFinished -= Handle;
			tcs.SetResult();
		}

		PlayScreenAnimation(id, targetsEnemy);
		AnimationFinished += Handle;
		return tcs.Task;
	}

	public Task WaitForReleaseEnergy()
	{
		TaskCompletionSource tcs = new();
		void Handle()
		{
			ReleaseEnergy.AnimationFinished -= Handle;
			ReleaseEnergy.Visible = false;
			tcs.SetResult();
		}

		ReleaseEnergy.Visible = true;
		AudioManager.Instance.PlaySFX("BA_release_energy", 1, 0.9f);
		ReleaseEnergy.Play();
		ReleaseEnergy.AnimationFinished += Handle;
		return tcs.Task;
	}

	private void StartAnimation(int id, Vector2 position, bool targetsEnemy)
	{
		if (!Animations.TryGetValue(id, out RPGMAnimatedSprite animation))
		{
			GD.PrintErr("Unknown animation: " + id);
			return;
		}

		Vector2 drawPosition = position - new Vector2(96f, 96f);
		// hack fix for the headbutt curtain animation
		if (id == 30)
			drawPosition += new Vector2(6f, 0f);

		int index = 0;
		switch (animation.Layer)
		{
			case 0:
				index = 10;
				break;
			case 2:
				index = -1;
				break;
			case 3:
				index = targetsEnemy ? -4 : 0;
				break;
		}

		if (animation.TryGetFrameSFX(0, out List<SFX> sfx))
		{
			sfx.ForEach(AudioManager.Instance.PlaySFX);
		}

		PlayingAnimation playing = new(animation, drawPosition, index);
		AddChild(playing);
		PlayingAnimations.Add(playing);
	}

	public IEnumerable<RPGMAnimatedSprite> GetAllAnimations()
	{
		return Animations.Values;
	}
}

#pragma warning disable CS0649
class AnimationInfo
{
	public int Id;
	public int Layer;
	public string Texture;
	public string AltTexture;
	public float[][][] Frames;
	public SFXInfo[] SFX;
	public ShakeInfo[] Shake;
}

class SFXInfo
{
	public int Frame;
	public string Name;
	public float Pitch;
	public float Volume;
}

class ShakeInfo
{
	public int Frame;
	public int Power;
	public int Speed;
	public int Duration;
}
#pragma warning restore CS0649
