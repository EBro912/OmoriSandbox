using Godot;

namespace OmoriSandbox;

internal partial class KeybindButton : Control
{
    [Export] public string AssociatedAction { get; private set; }
    [Export] public Key DefaultKey { get; private set; }
    public Key CurrentKey { get; private set; }
    private bool WaitingForInput = false;
    
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
            WaitingForInput = true;
            KeyButton.Text = "...";
        };
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