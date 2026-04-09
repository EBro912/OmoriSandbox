using System;
using Godot;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OmoriSandbox.Actors;

namespace OmoriSandbox;

/// <summary>
/// Handles displaying dialogue messages to the player. Mainly used for boss dialogue.
/// </summary>
public partial class DialogueManager : Node2D
{
	[Signal] public delegate void FinishedDialogueEventHandler();
	[Signal] public delegate void ChoiceSelectedEventHandler(bool choice);
	
	[Export] private RichTextLabel Text;
	[Export] private NinePatchRect Box;
	[Export] private Sprite2D SpeakerSprite;
	[Export] private Sprite2D Cursor;

	[Export] private NinePatchRect ChoiceBox;
	[Export] private Control ChoiceTextParent;

	private Queue<MessageBox> MessageQueue = [];
	private bool HasChoice = false;
	
	private const float TEXT_SPEED = 0.02f;
	private const int WIDTH = 328;
	private bool WaitingForInput = false;
	private bool IsTyping = false;
	private bool WaitingForAnimation = false;
	private bool WaitingForTimer = false;
	private bool WaitingForChoice = false;
	private double CharTimer = 0;
	private int CurrentMessageLength = 0;
	private int CharsTillSound = 2;
	private Dictionary<int, PauseType> PauseIndices = [];

	private Vector2I CursorNormalPos = new(145, 35);
	private Vector2I YesPos = new(100, -115);
	private Vector2I NoPos = new(100, -85);
	
    /// <summary>
    /// If dialogue is disabled in the current preset.<br/>
    /// Setting this value should be avoided unless necessary, as it can override preset settings.
    /// </summary>
    public bool DialogueDisabled = false;
	public static DialogueManager Instance { get; private set; }
	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Process(double delta)
	{
		if (!Visible)
			return;
		
		if (IsTyping)
		{
			if (Input.IsActionJustPressed("Accept") || Input.IsActionJustPressed("Back"))
			{
				SkipForward();
				return;
			}

			CharTimer += delta;
			if (CharTimer >= TEXT_SPEED)
			{
				CharTimer = 0;
				TypeChar();
			}
		}
		else if (WaitingForTimer)
		{
			if (Input.IsActionJustPressed("Accept") || Input.IsActionJustPressed("Back"))
				SkipForward();
		}
		else if (WaitingForInput)
		{
			if (Input.IsActionJustPressed("Accept") || Input.IsActionJustPressed("Back"))
			{
				WaitingForInput = false;
				if (Text.VisibleCharacters < CurrentMessageLength)
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
					AnimateClose();
				}
			}
			else if (Input.IsActionJustPressed("MenuUp") || Input.IsActionJustPressed("MenuDown"))
			{
				AudioManager.Instance.PlaySFX("SYS_move");
				Cursor.Position = Cursor.Position == YesPos ? NoPos : YesPos;
			}
		}
	}
	
	private void TypeChar()
	{
		if (Text.VisibleCharacters >= CurrentMessageLength)
		{
			IsTyping = false;
			if (HasChoice)
				WaitForChoice();
			else
				WaitForInput();
			return;
		}

		if (PauseIndices.TryGetValue(Text.VisibleCharacters, out PauseType p))
		{
			Text.VisibleCharacters++;
			IsTyping = false;
			switch (p)
			{
				case PauseType.QuarterSecond:
					WaitForTimer(0.25d);
					break;
				case PauseType.Second:
					WaitForTimer(1d);
					break;
				case PauseType.Input:
					WaitForInput();
					break;
				default:
					GD.PrintErr("Unhandled PauseType: " + p);
					break;
			}
			return;
		}

		Text.VisibleCharacters++;
		PlaySound();
	}

	private void PlaySound()
	{
		CharsTillSound--;
		if (CharsTillSound == 0)
		{
			AudioManager.Instance.PlaySFX("SYS_text", GameManager.Instance.Random.RandfRange(0.9f, 1.1f), 0.5f);
			CharsTillSound = 2;
		}
	}

	private void BeginMessage()
	{
		WaitingForAnimation = false;
		Text.Visible = true;
		Cursor.Visible = false;
		Cursor.Position = CursorNormalPos;
		MessageBox current = MessageQueue.Dequeue();
		PauseIndices.Clear();
		if (current.Speaker != null)
		{
			SpeakerSprite.Visible = true;        
			Vector2 local = ToLocal(current.SpeakerPos);
			SpeakerSprite.Position = new Vector2(Mathf.Clamp(local.X, -160, 160), SpeakerSprite.Position.Y);
			string cleaned = FindPauses(BuildHeader(FontType.Normal) + current.Speaker + ": " + BuildHeader(current.Font) + current.Message);
			Text.Text = cleaned;
			Text.VisibleCharacters = current.Speaker.Length + 2;
		}
		else
		{
			SpeakerSprite.Visible = false;
			string cleaned = FindPauses(BuildHeader(current.Font) + current.Message);
			Text.Text = cleaned;
			Text.VisibleCharacters = 0;
		}
		CurrentMessageLength = Text.GetTotalCharacterCount();
		HasChoice = current.HasChoice;
		IsTyping = true;
	}

	private string BuildHeader(FontType font)
	{
		StringBuilder sb = new();
		sb.Append("[font_size=28]");
		sb.Append(font is FontType.Normal
			? "[font=res://fonts/OMORI_GAME2.ttf]"
			: "[font=res://fonts/OMORI_GAME.ttf]");
		return sb.ToString();
	}

	private string FindPauses(string input)
	{
		StringBuilder sb = new();
		bool insideTag = false;
		bool insideSlash = false;
		// BBCode characters are not included in VisibleCharacters
		int visibleIndex = 0;

		foreach (char c in input)
		{
			if (c is '[')
			{
				insideTag = true;
				sb.Append(c);
				continue;
			}

			if (c is ']')
			{
				insideTag = false;
				sb.Append(c);
				continue;
			}

			if (!insideTag)
			{
				if (insideSlash)
				{
					switch (c)
					{
						case '.':
							PauseIndices.Add(visibleIndex, PauseType.QuarterSecond);
							break;
						case '|':
							PauseIndices.Add(visibleIndex, PauseType.Second);
							break;
						case '!':
							PauseIndices.Add(visibleIndex, PauseType.Input);
							break;
						default:
							GD.PushWarning("Invalid pause tag in dialogue: \\" + c);
							break;
					}
					insideSlash = false;
					continue;
				}
				
				if (c is '\\')
				{
					insideSlash = true;
					continue;
				}
				visibleIndex++;
			}

			sb.Append(c);
		}
		
		return sb.ToString();
	}

	private void SkipForward()
	{
		IsTyping = false;
		WaitingForTimer = false;

		int target = CurrentMessageLength;
		foreach (var pause in PauseIndices)
		{
			if (pause.Key >= Text.VisibleCharacters && pause.Value is PauseType.Input && pause.Key < target)
				target = pause.Key;
		}

		if (target < CurrentMessageLength)
		{
			// skip to just past the pause marker
			Text.VisibleCharacters = target + 1;
			WaitForInput();
		}
		else
		{
			Text.VisibleCharacters = CurrentMessageLength;
			if (HasChoice)
				WaitForChoice();
			else
				WaitForInput();
		}
	}

	private void WaitForInput()
	{
		WaitingForInput = true;
		Cursor.Visible = true;
	}

	private void WaitForTimer(double duration)
	{
		WaitingForTimer = true;
		GetTree().CreateTimer(duration).Timeout += () =>
		{
			if (!WaitingForTimer) return;
			WaitingForTimer = false;
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
    /// Waits for the user to select Yes/No.
    /// </summary>
    /// <remarks>If this method is not called after <see cref="QueueMessage"/>, the battle will continue while the choice is still on screen.<br/>
    /// If dialogue is disabled, this will always return true (yes).</remarks>
    /// <returns>True if the user picks Yes, False if the user picks No.</returns>
	public async Task<bool> WaitForUserChoice()
	{
		if (DialogueDisabled)
			return true;

		Variant[] args = await ToSignal(this, SignalName.ChoiceSelected);
		return (bool)args[0];
	}

	/// <summary>
	/// Queues a message to be displayed in the dialogue box.
	/// </summary>
	/// <param name="message">The message to display. The @ symbol can be used to pause mid-message.</param>
	/// <param name="hasChoice">If true, the message will come with a yes/no choice.</param>
	/// <param name="font">The Omori font to use, either Normal or Jagged.</param>
	public void QueueMessage(string message, bool hasChoice = false, FontType font = FontType.Normal)
	{
		QueueMessage(null, Vector2.Zero, message, hasChoice, font);
	}

    /// <summary>
    /// Queues a message to be displayed in the dialogue box, with an <see cref="Enemy"/> name as the speaker.<br/>
	/// The speaker arrow will point to the <see cref="Enemy"/>'s position on screen.
    /// </summary>
    /// <param name="speaker">The <see cref="Enemy"/> to show as the speaker.</param>
    /// <param name="message">The message to display. The @ symbol can be used to pause mid-message.</param>
    /// <param name="hasChoice">If true, the message will come with a yes/no choice.</param>
    /// <param name="font">The Omori font to use, either Normal or Jagged.</param>
    public void QueueMessage(Enemy speaker, string message, bool hasChoice = false, FontType font = FontType.Normal)
	{
		QueueMessage(speaker.Name, speaker.CenterPoint, message, hasChoice, font);
	}

    /// <summary>
    /// Queues a message to be displayed in the dialogue box, with a custom speaker name and position.<br/>
    /// </summary>
    /// <param name="speaker">The name of the speaker.</param>
    /// <param name="speakerPos">The position on screen to use as the speaker target.</param>
    /// <param name="message">The message to display. The @ symbol can be used to pause mid-message.</param>
    /// <param name="hasChoice">If true, the message will come with a yes/no choice.</param>
    /// <param name="font">The Omori font to use, either Normal or Jagged.</param>
    public void QueueMessage(string speaker, Vector2 speakerPos, string message, bool hasChoice = false, FontType font = FontType.Normal)
	{
		if (DialogueDisabled)
			return;

		MessageQueue.Enqueue(new MessageBox(speaker, speakerPos, message, hasChoice, font));
		
		if (WaitingForAnimation || IsTyping || WaitingForInput) 
			return;
		
		Visible = true;
		WaitingForAnimation = true;
		AnimateOpen();
	}

	private void AnimateOpen()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(Box, "custom_minimum_size:y", 110, 0.1f);
		tween.TweenCallback(Callable.From(BeginMessage));
	}

	private void AnimateChoiceOpen()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(ChoiceBox, "custom_minimum_size:y", 85, 0.1f);
		tween.TweenCallback(Callable.From(() =>
		{
			ChoiceTextParent.Visible = true;
			Cursor.Visible = true;
			Cursor.Position = YesPos;
		}));
	}

	private void AnimateClose()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(Box, "custom_minimum_size:y", 20, 0.1f);
		tween.TweenCallback(Callable.From(FinishMessage));
	}
	
	private void FinishMessage()
	{
		Visible = false;
		WaitingForAnimation = false;
		EmitSignal(SignalName.FinishedDialogue);
		if (HasChoice)
			EmitSignal(SignalName.ChoiceSelected, Cursor.Position == YesPos);
	}

	public void Reset()
	{
		MessageQueue.Clear();
		HasChoice = false;
		WaitingForAnimation = false;
		WaitingForInput = false;
		WaitingForChoice = false;
		WaitingForTimer = false;
		IsTyping = false;
		Cursor.Visible = false;
		CharsTillSound = 2;
		CurrentMessageLength = 0;
		CharTimer = 0;
	}

	private record MessageBox(string Speaker, Vector2 SpeakerPos, string Message, bool HasChoice, FontType Font);

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
		Jagged
	}

	private enum PauseType
	{
		QuarterSecond,
		Second,
		Input
	}
}
