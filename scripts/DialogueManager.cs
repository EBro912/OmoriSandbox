using Godot;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;
using OmoriSandbox.Editor;
using OmoriSandbox.Menu;

namespace OmoriSandbox;

/// <summary>
/// Handles displaying dialogue messages to the player. Mainly used for boss dialogue.
/// </summary>
public partial class DialogueManager : Node2D
{
	[Signal]
	public delegate void FinishedDialogueEventHandler();

	[Signal]
	public delegate void ChoiceSelectedEventHandler(string choice);

	[Export] private RichTextLabel Text;
	[Export] private NinePatchRect Box;
	[Export] private Sprite2D SpeakerSprite;
	[Export] private Sprite2D Cursor;

	[Export] private NinePatchRect ChoiceBox;
	[Export] private VBoxContainer ChoiceTextParent;

	// vanilla tints battlecards, energy bar, and battle log while a dialogue box is on screen
	[Export] private Color UIDimTint = new(0.5f, 0.5f, 0.5f);
	[Export] private float UIDimDuration = 0.2f;

	private Queue<MessageBox> MessageQueue = [];
	private bool HasChoice = false;

	private const float TEXT_SPEED = 1f / 60f;
	private const float TEXT_SIZE = 32.5f;
	private bool WaitingForInput = false;
	private bool IsTyping = false;
	private bool WaitingForAnimation = false;
	private bool WaitingForTimer = false;
	private bool WaitingForChoice = false;
	private double CharTimer = 0;
	private int CurrentMessageLength = 0;
	private int CharsTillSound = 3;
	private Dictionary<int, List<Pause>> PauseIndices = [];
	private RichTextLabel Measure;
	private Tween OpenCloseTween;
	private Tween DimTween;
	private Tween CursorFadeTween;
	private bool UIDimmed;

	private static readonly string[] ParagraphTags = ["[center]", "[right]", "[fill]", "[left]"];

	private Vector2 CursorNormalPos = new(140, 37.5f);
	private string[] CurrentChoices;
	private int ChoiceIndex = 0;
	private readonly MenuRepeat ChoiceRepeat = new();

	private const float CHOICE_BOX_RIGHT = 180f;
	private const float CHOICE_BOX_DEFAULT_LEFT = 70f;
	private const float CHOICE_BOX_BOTTOM = -60f;
	private const float CURSOR_TOP_OFFSET = 30f;
	private const float CURSOR_LEFT_PADDING = 30f;
	private const float CURSOR_FADE_DURATION = 10f / 60f;

	/// <summary>
	/// If dialogue is disabled in the current preset.<br/>
	/// Setting this value should be avoided unless necessary, as it can override preset settings.
	/// </summary>
	public bool DialogueDisabled = false;

	public static DialogueManager Instance { get; private set; }
	
	private Vector2 DefaultPosition;
	private const float NoEnergyBarOffset = 54f;

	// whether the current message closes/advances itself when it finishes typing via the \^ marker, unless it has choices
	private bool AutoClose;
	private bool TextFinished;
	private bool ShowFast;
	private double TriggerHeldTime;

	public override void _EnterTree()
	{
		Instance = this;
		DefaultPosition = Position;
	}

	public override void _Process(double delta)
	{
		bool triggered = UpdateTriggered(delta);
		if (!Visible)
			return;

		if (IsTyping)
		{
			if (triggered && SettingsMenuManager.Instance.TextSkip)
				ShowFast = true;

			if (ShowFast || SettingsMenuManager.Instance.InstantDialogue)
			{
				// type the whole message at once, scripted pauses and input waits still apply
				while (IsTyping)
					TypeChar();
				CharTimer = 0;
				return;
			}

			double delay = TEXT_SPEED / SettingsMenuManager.Instance.DialogueSpeed;
			CharTimer += delta;
			// when the delay is shorter than a frame, one char per frame can't keep up
			// type enough chars this frame to hold the configured rate
			while (CharTimer >= delay && IsTyping)
			{
				CharTimer -= delay;
				TypeChar();
			}
			if (!IsTyping)
				CharTimer = 0;
		}
		else if (WaitingForInput)
		{
			if (triggered && !CursorFadeTween.IsRunning())
			{
				WaitingForInput = false;
				if (!TextFinished)
				{
					Cursor.Visible = false;
					IsTyping = true;
				}
				else if (MessageQueue.Count == 0)
				{
					Cursor.Visible = false;
					SpeakerSprite.Visible = false;
					Text.Visible = false;
					WaitingForAnimation = true;
					AnimateClose();
				}
				else
				{
					BeginMessage();
				}
			}
		}
		else if (WaitingForChoice)
		{
			ChoiceRepeat.Update(delta);
			if (Input.IsActionJustPressed("Accept"))
			{
				AudioManager.Instance.PlaySFX("SYS_select");
				WaitingForChoice = false;
				if (MessageQueue.Count == 0)
				{
					Cursor.Visible = false;
					SpeakerSprite.Visible = false;
					Text.Visible = false;
					ChoiceBox.Visible = false;
					ChoiceTextParent.Visible = false;
					ChoiceBox.CustomMinimumSize = new Vector2(110, 20);
					ChoiceBox.OffsetLeft = CHOICE_BOX_DEFAULT_LEFT;
					AnimateClose();
				}
				else
				{
					// more messages are queued, emit the choice now and move on
					Cursor.Visible = false;
					ChoiceBox.Visible = false;
					ChoiceTextParent.Visible = false;
					ChoiceBox.CustomMinimumSize = new Vector2(110, 20);
					ChoiceBox.OffsetLeft = CHOICE_BOX_DEFAULT_LEFT;
					// capture before BeginMessage overwrites the choice state
					string choice = CurrentChoices[ChoiceIndex];
					HasChoice = false;
					EmitSignal(SignalName.ChoiceSelected, choice);
					BeginMessage();
				}
			}
			else if (ChoiceRepeat.Direction == Vector2I.Up)
			{
				if (ChoiceIndex > 0 || (ChoiceRepeat.Fresh && CurrentChoices.Length > 1))
				{
					ChoiceIndex = (ChoiceIndex - 1 + CurrentChoices.Length) % CurrentChoices.Length;
					Cursor.Position = GetCursorPosition(ChoiceIndex);
					AudioManager.Instance.PlaySFX("SYS_move");
				}
			}
			else if (ChoiceRepeat.Direction == Vector2I.Down)
			{
				if (ChoiceIndex < CurrentChoices.Length - 1 || (ChoiceRepeat.Fresh && CurrentChoices.Length > 1))
				{
					ChoiceIndex = (ChoiceIndex + 1) % CurrentChoices.Length;
					Cursor.Position = GetCursorPosition(ChoiceIndex);
					AudioManager.Instance.PlaySFX("SYS_move");
				}
			}
		}
	}

	private bool UpdateTriggered(double delta)
	{
		if (Input.IsActionJustPressed("Accept") || Input.IsActionJustPressed("Back"))
		{
			TriggerHeldTime = 0;
			return true;
		}
		if (!Input.IsActionPressed("Accept") && !Input.IsActionPressed("Back"))
			return false;
		int before = (int)(TriggerHeldTime * 60 + 0.01);
		TriggerHeldTime += delta;
		int after = (int)(TriggerHeldTime * 60 + 0.01);
		for (int frame = before + 1; frame <= after; frame++)
		{
			if (frame >= 24 && frame % 6 == 0)
				return true;
		}
		return false;
	}

	private void TypeChar()
	{
		if (PauseIndices.TryGetValue(Text.VisibleCharacters, out List<Pause> pauses))
		{
			Pause p = pauses[0];
			pauses.RemoveAt(0);
			// the pause sits before the character at this index, which stays hidden until typing resumes
			if (pauses.Count == 0)
				PauseIndices.Remove(Text.VisibleCharacters);
			if (pauses.Count == 0 && p.Type == PauseType.Input && Text.VisibleCharacters >= CurrentMessageLength && !AutoClose)
				return;
			IsTyping = false;
			switch (p.Type)
			{
				case PauseType.Wait:
					WaitForTimer(p.Frames / 60d);
					break;
				case PauseType.Input:
					WaitForInput();
					break;
				default:
					GD.PrintErr("Unhandled PauseType: " + p.Type);
					break;
			}

			return;
		}

		if (Text.VisibleCharacters >= CurrentMessageLength)
		{
			IsTyping = false;
			EndOfMessage();
			return;
		}

		Text.VisibleCharacters++;
		PlaySound();
	}

	private ulong LastSoundFrame = ulong.MaxValue;

	private void PlaySound()
	{
		CharsTillSound--;
		if (CharsTillSound == 0)
		{
			CharsTillSound = 3;
			// at high dialogue speeds multiple chars type in one frame, cap at one blip per frame
			ulong frame = Engine.GetProcessFrames();
			if (frame == LastSoundFrame)
				return;
			LastSoundFrame = frame;
			AudioManager.Instance.PlaySFX("SYS_text", GameManager.Instance.Random.RandfRange(0.9f, 1.1f), 0.5f);
		}
	}

	private void BeginMessage()
	{
		// a Reset may have emptied the queue while the open animation was still pending
		if (MessageQueue.Count == 0)
			return;
		WaitingForAnimation = false;
		Text.Visible = true;
		Cursor.Visible = false;
		Cursor.Position = CursorNormalPos;
		MessageBox current = MessageQueue.Dequeue();
		PauseIndices.Clear();
		AutoClose = false;
		TextFinished = false;
		ShowFast = false;
		if (current.SpeakerPos != default)
		{
			SpeakerSprite.Visible = true;
			Vector2 local = ToLocal(current.SpeakerPos);
			SpeakerSprite.Position = new Vector2(Mathf.Clamp(local.X, -160, 160), SpeakerSprite.Position.Y);
		}
		else if (current.Speaker == null)
		{
			SpeakerSprite.Visible = false;
		}

		if (current.Speaker != null)
		{
			string cleaned = FindPauses(BuildHeader(FontType.Normal) + current.Speaker + ": " + BuildHeader(current.Font) + current.Message);
			Text.Text = cleaned;
			Text.VisibleCharacters = current.Speaker.Length + 2;
		}
		else
		{
			// in Godot 4.7+, a paragraph tag after any inline tag starts a second paragraph,
			// leaving an empty full-height first line, so alignment tags stay ahead of the header
			// so we need to fix that here
			string message = current.Message;
			string alignment = "";
			foreach (string tag in ParagraphTags)
			{
				if (message.StartsWith(tag))
				{
					alignment = tag;
					message = message[tag.Length..];
					break;
				}
			}
			string cleaned = FindPauses(alignment + BuildHeader(current.Font) + message);
			Text.Text = cleaned;
			Text.VisibleCharacters = 0;
		}

		CurrentMessageLength = Text.GetTotalCharacterCount();
		HasChoice = current.Choices is { Length: > 0 };
		if (HasChoice)
		{
			CurrentChoices = current.Choices;
			ChoiceIndex = 0;
			ClearChoiceLabels();
			foreach (string choice in current.Choices)
				ChoiceTextParent.AddChild(CreateChoiceLabel(choice));
		}

		IsTyping = true;
	}

	private string BuildHeader(FontType font)
	{
		StringBuilder sb = new();
		sb.Append("[font_size=28]");
		sb.Append(font switch
		{
			FontType.Jagged => "[font=res://fonts/OMORI_GAME.ttf]",
			FontType.NotoSans => "[font=res://fonts/NotoSans_Regular.ttf]",
			_ => "[font=res://fonts/OMORI_GAME2.ttf]"
		});
		return sb.ToString();
	}

	private string FindPauses(string input)
	{
		StringBuilder sb = new();
		List<(int Offset, Pause Pause)> found = [];
		bool insideTag = false;

		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (!insideTag && c is '\\')
			{
				i = ReadEscape(input, i, sb, found);
				continue;
			}

			if (c is '[')
				insideTag = true;
			else if (c is ']')
				insideTag = false;

			sb.Append(c);
		}

		// Godot treats BBCode tags as a single "character"
		// we can use a fake text label to measure pauses before displaying
		// unlike Omori, we don't actually render any text
		// so the performance impact is marginal at best
		string bbcode = sb.ToString();
		if (Measure == null)
		{
			Measure = new RichTextLabel { BbcodeEnabled = true, Visible = false };
			AddChild(Measure);
		}
		Measure.CustomEffects = Text.CustomEffects;
		Measure.Clear();
		int start = 0;
		foreach ((int offset, Pause pause) in found)
		{
			Measure.AppendText(bbcode[start..offset]);
			start = offset;
			AddPause(Measure.GetTotalCharacterCount(), pause);
		}
		return bbcode;
	}

	private int ReadEscape(string input, int slash, StringBuilder sb, List<(int Offset, Pause Pause)> found)
	{
		int i = slash + 1;
		if (i >= input.Length)
			return slash;

		switch (input[i])
		{
			case '.':
				found.Add((sb.Length, new Pause(PauseType.Wait, 15)));
				return i;
			case '|':
				found.Add((sb.Length, new Pause(PauseType.Wait, 60)));
				return i;
			case '!':
				found.Add((sb.Length, new Pause(PauseType.Input, 0)));
				return i;
			case '^':
				AutoClose = true;
				return i;
			case '\\':
				sb.Append('\\');
				return i;
		}

		if (!char.IsAsciiLetter(input[i]))
		{
			GD.PushWarning("Invalid pause tag in dialogue: \\" + input[i]);
			return i;
		}

		int end = i;
		while (end < input.Length && char.IsAsciiLetter(input[end]))
			end++;
		string code = input[i..end].ToUpperInvariant();
		int last = end - 1;
		int frames = 0;
		if (end < input.Length && input[end] == '[')
		{
			int close = end + 1;
			while (close < input.Length && char.IsAsciiDigit(input[close]))
				close++;
			if (close > end + 1 && close < input.Length && input[close] == ']')
			{
				int.TryParse(input[(end + 1)..close], out frames);
				last = close;
			}
		}

		if (code == "W")
		{
			if (frames > 0)
				found.Add((sb.Length, new Pause(PauseType.Wait, frames)));
		}
		else
		{
			GD.PushWarning("Invalid pause tag in dialogue: \\" + code);
		}
		return last;
	}

	private void AddPause(int index, Pause pause)
	{
		if (!PauseIndices.TryGetValue(index, out List<Pause> list))
		{
			list = [];
			PauseIndices[index] = list;
		}
		list.Add(pause);
	}

	private void EndOfMessage()
	{
		TextFinished = true;
		if (HasChoice)
			WaitForChoice();
		else if (AutoClose)
			AdvanceAutoClose();
		else
			WaitForInput();
	}

	// closes or advances a \^ message without waiting for input
	private void AdvanceAutoClose()
	{
		if (MessageQueue.Count == 0)
		{
			Cursor.Visible = false;
			SpeakerSprite.Visible = false;
			Text.Visible = false;
			WaitingForAnimation = true;
			// FinishMessage emits FinishedDialogue
			AnimateClose();
		}
		else
		{
			BeginMessage();
		}
	}

	private void WaitForInput()
	{
		WaitingForInput = true;
		Cursor.Visible = true;
		Cursor.Modulate = new Color(1, 1, 1, 0);
		CursorFadeTween?.Kill();
		CursorFadeTween = CreateTween();
		CursorFadeTween.TweenProperty(Cursor, "modulate:a", 1f, CURSOR_FADE_DURATION);
	}

	// invalidates pending pause timers from earlier messages/dialogues
	private int PauseGeneration = 0;

	private void WaitForTimer(double duration)
	{
		WaitingForTimer = true;
		int gen = ++PauseGeneration;
		GetTree().CreateTimer(duration).Timeout += () =>
		{
			if (gen != PauseGeneration || !WaitingForTimer) return;
			WaitingForTimer = false;
			if (!Visible) return;
			IsTyping = true;
		};
	}

	private void WaitForChoice()
	{
		WaitingForChoice = true;
		ChoiceBox.Visible = true;
		AnimateChoiceOpen();
	}

	/// <summary>
	/// Waits for the current dialogue to finish and for the player to dismiss it.
	/// </summary>
	/// <remarks>
	/// If this method is not called after <see cref="QueueMessage"/>, the battle will continue while the dialogue is still on screen.
	/// </remarks>
	public async Task WaitForDialogue()
	{
		if (DialogueDisabled || !Visible)
			return;

		await ToSignal(this, SignalName.FinishedDialogue);
	}

	/// <summary>
	/// Waits for the user to select a choice.
	/// </summary>
	/// <remarks>If this method is not called after <see cref="QueueMessage"/>, the battle will continue while the choice is still on screen.<br/>
	/// If dialogue is disabled, this will always return the first option.</remarks>
	/// <returns>The selected choice string.</returns>
	public async Task<string> WaitForUserChoice()
	{
		if (DialogueDisabled)
			return CurrentChoices?[0];

		Variant[] args = await ToSignal(this, SignalName.ChoiceSelected);
		return (string)args[0];
	}

	/// <summary>
	/// Queues a message to be displayed in the dialogue box.
	/// </summary>
	/// <param name="message">The message to display. Supports the following pause markers: <c>\.</c> pauses for 15 frames (0.25s), <c>\|</c> pauses for 60 frames (1s),
	/// <c>\W[n]</c> pauses for n frames, <c>\!</c> waits for input, and <c>\^</c> closes (or advances) the box automatically once it finishes, unless it has choices.</param>
	/// <param name="choices">A list of choices this dialogue box has.</param>
	/// <param name="font">The <see cref="FontType"/> to use: Normal, Jagged or NotoSans.</param>
	public void QueueMessage(string message, string[] choices = null, FontType font = FontType.Normal)
	{
		QueueMessage(null, Vector2.Zero, message, choices, font);
	}

	/// <summary>
	/// Queues a message to be displayed in the dialogue box with no speaker name, with the speaker arrow pointing at a position on screen.
	/// </summary>
	/// <remarks>
	/// Pass an enemy's <see cref="Actor.CenterPoint"/> to point at it. A position of <see cref="Vector2.Zero"/> hides the arrow.
	/// </remarks>
	/// <param name="speakerPos">The position on screen to use as the speaker target.</param>
	/// <param name="message">The message to display. Supports the following pause markers: <c>\.</c> pauses for 15 frames (0.25s), <c>\|</c> pauses for 60 frames (1s),
	/// <c>\W[n]</c> pauses for n frames, <c>\!</c> waits for input, and <c>\^</c> closes (or advances) the box automatically once it finishes, unless it has choices.</param>
	/// <param name="choices">A list of choices this dialogue box has.</param>
	/// <param name="font">The <see cref="FontType"/> to use: Normal, Jagged or NotoSans.</param>
	public void QueueMessage(Vector2 speakerPos, string message, string[] choices = null, FontType font = FontType.Normal)
	{
		QueueMessage(null, speakerPos, message, choices, font);
	}

	/// <summary>
	/// Queues a message to be displayed in the dialogue box, with an <see cref="Enemy"/> name as the speaker.<br/>
	/// The speaker arrow will point to the <see cref="Enemy"/>'s position on screen.
	/// </summary>
	/// <param name="speaker">The <see cref="Enemy"/> to show as the speaker.</param>
	/// <param name="message">The message to display. Supports the following pause markers: <c>\.</c> pauses for 15 frames (0.25s), <c>\|</c> pauses for 60 frames (1s),
	/// <c>\W[n]</c> pauses for n frames, <c>\!</c> waits for input, and <c>\^</c> closes (or advances) the box automatically once it finishes, unless it has choices.</param>
	/// <param name="choices">A list of choices this dialogue box has.</param>
	/// <param name="font">The <see cref="FontType"/> to use: Normal, Jagged or NotoSans.</param>
	public void QueueMessage(Enemy speaker, string message, string[] choices = null, FontType font = FontType.Normal)
	{
		QueueMessage(speaker.Name, speaker.CenterPoint, message, choices, font);
	}

	/// <summary>
	/// Queues a message to be displayed in the dialogue box, with a custom speaker name and position.<br/>
	/// </summary>
	/// <param name="speaker">The name of the speaker.</param>
	/// <param name="speakerPos">The position on screen to use as the speaker target.</param>
	/// <param name="message">The message to display. Supports the following pause markers: <c>\.</c> pauses for 15 frames (0.25s), <c>\|</c> pauses for 60 frames (1s),
	/// <c>\W[n]</c> pauses for n frames, <c>\!</c> waits for input, and <c>\^</c> closes (or advances) the box automatically once it finishes, unless it has choices.</param>
	/// <param name="choices">A list of choices this dialogue box has.</param>
	/// <param name="font">The <see cref="FontType"/> to use: Normal, Jagged or NotoSans.</param>
	public void QueueMessage(string speaker, Vector2 speakerPos, string message, string[] choices = null,
		FontType font = FontType.Normal)
	{
		CurrentChoices = choices;
		if (DialogueDisabled)
			return;

		MessageQueue.Enqueue(new MessageBox(speaker, speakerPos, message, choices, font));

		if (WaitingForAnimation || IsTyping || WaitingForInput || WaitingForTimer || WaitingForChoice)
			return;
		
		bool barVisible = MenuManager.Instance != null && MenuManager.Instance.EnergyDisplay.Visible;
		Position = barVisible ? DefaultPosition : DefaultPosition + new Vector2(0, NoEnergyBarOffset);

		Visible = true;
		WaitingForAnimation = true;
		SetUIDimmed(true);
		AnimateOpen();
	}

	/// <summary>
	/// Queues a message to be displayed in the dialogue box, with a custom speaker name.
	/// </summary>
	/// <param name="speaker">The name of the speaker.</param>
	/// <param name="message">The message to display. Supports the following pause markers: <c>\.</c> pauses for 15 frames (0.25s), <c>\|</c> pauses for 60 frames (1s),
	/// <c>\W[n]</c> pauses for n frames, <c>\!</c> waits for input, and <c>\^</c> closes (or advances) the box automatically once it finishes, unless it has choices.</param>
	/// <param name="choices">A list of choices this dialogue box has.</param>
	/// <param name="font">The <see cref="FontType"/> to use: Normal, Jagged or NotoSans.</param>
	public void QueueMessage(string speaker, string message, string[] choices = null, FontType font = FontType.Normal)
	{
		QueueMessage(speaker, default, message, choices, font);
	}


	private void AnimateOpen()
	{
		OpenCloseTween = CreateTween();
		OpenCloseTween.TweenProperty(Box, "custom_minimum_size:y", 112, 0.1f);
		OpenCloseTween.TweenCallback(Callable.From(BeginMessage));
	}

	private void AnimateChoiceOpen()
	{
		float targetHeight = 20 + TEXT_SIZE * CurrentChoices.Length;

		Font font = ResourceLoader.Load<Font>("res://fonts/OMORI_GAME2.ttf");
		float maxTextWidth = 0f;
		foreach (string choice in CurrentChoices)
		{
			float w = font.GetStringSize(ParsedChoiceText(choice), fontSize: 30).X;
			maxTextWidth = Mathf.Max(maxTextWidth, w);
		}
		float targetWidth = Mathf.Max(110f, maxTextWidth + 34f + CURSOR_LEFT_PADDING);

		ChoiceBox.CustomMinimumSize = new Vector2(targetWidth, ChoiceBox.CustomMinimumSize.Y);
		ChoiceBox.OffsetLeft = CHOICE_BOX_RIGHT - targetWidth;
		ChoiceTextParent.Visible = false;

		Tween tween = CreateTween();
		tween.TweenProperty(ChoiceBox, "custom_minimum_size:y", targetHeight, 0.1f);
		tween.TweenCallback(Callable.From(() =>
		{
			ChoiceTextParent.Visible = true;
			CursorFadeTween?.Kill();
			Cursor.Modulate = Colors.White;
			Cursor.Visible = true;
			Cursor.Position = GetCursorPosition(0);
		}));
	}

	internal RichTextLabel CreateChoiceLabel(string choice)
	{
		RichTextLabel label = new()
		{
			BbcodeEnabled = true,
			FitContent = true,
			AutowrapMode = TextServer.AutowrapMode.Off,
			ScrollActive = false,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Text = choice
		};
		label.AddThemeFontOverride("normal_font", ResourceLoader.Load<Font>("res://fonts/OMORI_GAME2.ttf"));
		label.AddThemeFontSizeOverride("normal_font_size", 30);
		label.AddThemeColorOverride("default_color", ChoiceTextParent.GetThemeColor("font_color", "Label"));
		return label;
	}

	internal string ParsedChoiceText(string choice)
	{
		RichTextLabel parser = new() { BbcodeEnabled = true, Text = choice };
		string parsed = parser.GetParsedText();
		parser.Free();
		return parsed;
	}

	private Vector2 GetCursorPosition(int index)
	{
		float left = CHOICE_BOX_RIGHT - ChoiceBox.CustomMinimumSize.X;
		float boxTop = CHOICE_BOX_BOTTOM - ChoiceBox.CustomMinimumSize.Y;
		return new Vector2(left + CURSOR_LEFT_PADDING, boxTop + CURSOR_TOP_OFFSET + TEXT_SIZE * index);
	}

	private void ClearChoiceLabels()
	{
		foreach (Node child in ChoiceTextParent.GetChildren())
			child.QueueFree();
	}

	private void AnimateClose()
	{
		OpenCloseTween = CreateTween();
		OpenCloseTween.TweenProperty(Box, "custom_minimum_size:y", 20, 0.1f);
		OpenCloseTween.TweenCallback(Callable.From(FinishMessage));
	}
	
	private void FinishMessage()
	{
		Visible = false;
		WaitingForAnimation = false;
		SetUIDimmed(false);
		EmitSignal(SignalName.FinishedDialogue);
		if (HasChoice)
			EmitSignal(SignalName.ChoiceSelected, CurrentChoices[ChoiceIndex]);
	}
	
	private static IEnumerable<CanvasItem> GetDimTargets()
	{
		if (BattleManager.Instance != null)
		{
			foreach (PartyMember member in BattleManager.Instance.GetAllPartyMembers())
			{
				Control battlecard = member.Component?.Battlecard;
				if (battlecard != null)
					yield return battlecard;
			}
		}
		if (MenuManager.Instance != null)
			yield return MenuManager.Instance.EnergyDisplay;
		if (BattleLogManager.Instance != null)
			yield return BattleLogManager.Instance;
	}

	private void SetUIDimmed(bool dimmed)
	{
		if (UIDimmed == dimmed)
			return;
		UIDimmed = dimmed;
		Color target = dimmed ? UIDimTint : Colors.White;
		DimTween?.Kill();
		DimTween = CreateTween().SetParallel();
		foreach (CanvasItem item in GetDimTargets())
			DimTween.TweenProperty(item, "modulate", target, UIDimDuration).SetTrans(Tween.TransitionType.Sine);
		if (dimmed)
			AnimationManager.Instance.SetUIToneDimmed(true);
		else
			DimTween.Chain().TweenCallback(Callable.From(() => AnimationManager.Instance.SetUIToneDimmed(false)));
	}

	public void Reset()
	{
		MessageQueue.Clear();
		OpenCloseTween?.Kill();
		// restore UI tint immediately
		DimTween?.Kill();
		UIDimmed = false;
		foreach (CanvasItem item in GetDimTargets())
			item.Modulate = Colors.White;
		AnimationManager.Instance.SetUIToneDimmed(false);
		PauseGeneration++;
		HasChoice = false;
		AutoClose = false;
		TextFinished = false;
		ShowFast = false;
		Position = DefaultPosition;
		WaitingForAnimation = false;
		WaitingForInput = false;
		WaitingForChoice = false;
		WaitingForTimer = false;
		IsTyping = false;
		Cursor.Visible = false;
		CharsTillSound = 3;
		CurrentMessageLength = 0;
		CharTimer = 0;
		ChoiceIndex = 0;
		CurrentChoices = null;
		ClearChoiceLabels();
		ChoiceBox.CustomMinimumSize = new Vector2(110, 20);
		ChoiceBox.OffsetLeft = CHOICE_BOX_DEFAULT_LEFT;
		// hide everything and restore the box height for the next dialogue
		Visible = false;
		Text.Visible = false;
		Text.Text = "";
		SpeakerSprite.Visible = false;
		ChoiceBox.Visible = false;
		ChoiceTextParent.Visible = false;
		Box.CustomMinimumSize = new Vector2(Box.CustomMinimumSize.X, 20);
	}

	private record MessageBox(string Speaker, Vector2 SpeakerPos, string Message, string[] Choices, FontType Font);

	/// <summary>
	/// The default Omori font types to use in dialogue boxes.
	/// To set your own font, use the BBCode [font] tag.
	/// </summary>
	public enum FontType
	{
		/// <summary>
		/// The normal font used in regular text.
		/// </summary>
		Normal,
		/// <summary>
		/// The jagged font used in horror text.
		/// </summary>
		Jagged,
		/// <summary>
		/// The default RPGMaker font.
		/// </summary>
		NotoSans
	}

	private enum PauseType
	{
		Wait,
		Input
	}

	private readonly record struct Pause(PauseType Type, int Frames);
}
