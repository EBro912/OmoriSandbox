using Godot;

namespace OmoriSandbox;

internal partial class KeybindButton : Control
{
    [Export] public string AssociatedAction { get; private set; }
    [Export] public Key DefaultKey { get; private set; }
    public Key CurrentKey { get; private set; }
    private bool WaitingForInput = false;
    private const double WaitTimeout = 10d;
    private double WaitTime = 0;

    // only one keybind may capture input at a time
    private static KeybindButton CurrentlyCapturing;
    internal static bool IsCapturing => CurrentlyCapturing != null;

    private Button KeyButton;

    public override void _Ready()
    {
        GetNode<Label>("ActionLabel").Text = AssociatedAction;
        KeyButton = GetNode<Button>("KeyButton");
        KeyButton.Text = OS.GetKeycodeString(CurrentKey);
        KeyButton.Pressed += () =>
        {
            if (WaitingForInput)
                return;
            CurrentlyCapturing?.CancelCapture();
            CurrentlyCapturing = this;
            WaitingForInput = true;
            KeyButton.Text = "...";
        };
    }

    private void CancelCapture()
    {
        WaitingForInput = false;
        WaitTime = 0;
        KeyButton.Text = OS.GetKeycodeString(CurrentKey);
        if (CurrentlyCapturing == this)
            CurrentlyCapturing = null;
    }

    public override void _Process(double delta)
    {
        if (!WaitingForInput)
            return;

        WaitTime += delta;
        if (WaitTime >= WaitTimeout)
            CancelCapture();
    }

    public override void _Input(InputEvent @event)
    {
        if (!WaitingForInput)
            return;

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            CurrentKey = keyEvent.Keycode;
            UpdateKeybind();
            KeyButton.Text = OS.GetKeycodeString(CurrentKey);
            WaitingForInput = false;
            WaitTime = 0;
            if (CurrentlyCapturing == this)
                CurrentlyCapturing = null;
            // don't let the captured key also trigger whatever it is bound to
            GetViewport().SetInputAsHandled();
        }
    }

    private void UpdateKeybind()
    {
        foreach (InputEvent ev in InputMap.ActionGetEvents(AssociatedAction))
            InputMap.ActionEraseEvent(AssociatedAction, ev);
        InputMap.ActionAddEvent(AssociatedAction, new InputEventKey
        {
            Keycode = CurrentKey
        });
    }

    public void SetKey(Key key)
    {
        CurrentKey = key;
        KeyButton.Text = OS.GetKeycodeString(CurrentKey);
        UpdateKeybind();
    }

    public void Reset()
    {
        CurrentKey = DefaultKey;
        KeyButton.Text = OS.GetKeycodeString(CurrentKey);
        UpdateKeybind();
    }
}