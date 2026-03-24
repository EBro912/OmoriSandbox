using Godot;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Editor;

internal partial class AnimationViewer : Control
{
    [Export] private SpinBox AnimationIdSelector;
    [Export] private Button PlayButton;
    [Export] private Node PreviewRoot;
    [Export] private SpinBox LayerSelector;

    private PlayingAnimation Animation;

    public override void _Ready()
    {
        PlayButton.Pressed += () =>
        {
            Animation = AnimationManager.Instance.PreviewAnimation((int)AnimationIdSelector.Value, (int)LayerSelector.Value);
            if (Animation != null)
            {
                // we need to play the animation on this canvas instead of the battle one
                PreviewRoot.AddChild(Animation);
                PlayButton.Disabled = true;
            }
        };

        AnimationManager.Instance.AnimationFinished += () =>
        {
            PlayButton.Disabled = false;
            Animation = null;
        };
    }
}