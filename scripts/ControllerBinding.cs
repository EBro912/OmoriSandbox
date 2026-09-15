using Godot;
using Godot.Collections;

namespace OmoriSandbox;

internal static class ControllerBinding
{
    internal static bool IsController(InputEvent input) => input is InputEventJoypadButton or InputEventJoypadMotion;

    internal static InputEvent Normalize(InputEvent input) => input switch
    {
        InputEventJoypadButton button when button.ButtonIndex >= 0 && button.ButtonIndex < JoyButton.Max =>
            new InputEventJoypadButton { Device = -1, ButtonIndex = button.ButtonIndex },
        InputEventJoypadMotion motion when motion.Axis >= 0 && motion.Axis < JoyAxis.Max && motion.AxisValue != 0 =>
            new InputEventJoypadMotion { Device = -1, Axis = motion.Axis, AxisValue = Mathf.Sign(motion.AxisValue) },
        _ => null
    };

    internal static void UpdateAction(string action, InputEvent binding)
    {
        ReplaceEvents(action, binding);
        // convert sandbox actions to godot actions
        string uiAction = action switch
        {
            "Accept" => "ui_accept",
            "Back" => "ui_cancel",
            "MenuUp" => "ui_up",
            "MenuDown" => "ui_down",
            "MenuLeft" => "ui_left",
            "MenuRight" => "ui_right",
            _ => null
        };
        if (uiAction != null)
            ReplaceEvents(uiAction, binding);
    }

    private static void ReplaceEvents(string action, InputEvent binding)
    {
        foreach (InputEvent input in InputMap.ActionGetEvents(action))
        {
            if (IsController(input))
                InputMap.ActionEraseEvent(action, input);
        }
        if (binding != null)
            InputMap.ActionAddEvent(action, binding);
    }

    internal static Dictionary Serialize(InputEvent binding) => binding switch
    {
        InputEventJoypadButton button => new Dictionary { ["button"] = (int)button.ButtonIndex },
        InputEventJoypadMotion motion => new Dictionary { ["axis"] = (int)motion.Axis, ["direction"] = Mathf.Sign(motion.AxisValue) },
        _ => new Dictionary()
    };

    internal static bool TryDeserialize(Variant value, out InputEvent binding)
    {
        binding = null;
        if (value.VariantType != Variant.Type.Dictionary)
            return false;
        Dictionary data = value.AsGodotDictionary();
        
        if (data.Count == 0)
            return true;
        if (data.TryGetValue("button", out Variant button) && button.VariantType == Variant.Type.Int &&
            button.AsInt64() >= 0 && button.AsInt64() < (int)JoyButton.Max)
        {
            binding = new InputEventJoypadButton { Device = -1, ButtonIndex = (JoyButton)button.AsInt32() };
            return true;
        }
        if (data.TryGetValue("axis", out Variant axis) && axis.VariantType == Variant.Type.Int &&
            axis.AsInt64() >= 0 && axis.AsInt64() < (int)JoyAxis.Max &&
            data.TryGetValue("direction", out Variant direction) && direction.VariantType == Variant.Type.Int &&
            direction.AsInt64() is -1 or 1)
        {
            binding = new InputEventJoypadMotion { Device = -1, Axis = (JoyAxis)axis.AsInt32(), AxisValue = direction.AsInt32() };
            return true;
        }
        return false;
    }
    
    internal static string DisplayName(InputEvent binding) => binding switch
    {
        InputEventJoypadButton button => button.ButtonIndex switch
        {
            JoyButton.A => "A",
            JoyButton.B => "B",
            JoyButton.X => "X",
            JoyButton.Y => "Y",
            JoyButton.Back => "Back",
            JoyButton.Guide => "Guide",
            JoyButton.Start => "Start",
            JoyButton.LeftStick => "LS Click",
            JoyButton.RightStick => "RS Click",
            JoyButton.LeftShoulder => "LB",
            JoyButton.RightShoulder => "RB",
            JoyButton.DpadUp => "D-Pad Up",
            JoyButton.DpadDown => "D-Pad Down",
            JoyButton.DpadLeft => "D-Pad Left",
            JoyButton.DpadRight => "D-Pad Right",
            _ => $"Button {(int)button.ButtonIndex}"
        },
        InputEventJoypadMotion motion => motion.Axis switch
        {
            JoyAxis.LeftX => motion.AxisValue < 0 ? "LS Left" : "LS Right",
            JoyAxis.LeftY => motion.AxisValue < 0 ? "LS Up" : "LS Down",
            JoyAxis.RightX => motion.AxisValue < 0 ? "RS Left" : "RS Right",
            JoyAxis.RightY => motion.AxisValue < 0 ? "RS Up" : "RS Down",
            JoyAxis.TriggerLeft => "LT",
            JoyAxis.TriggerRight => "RT",
            _ => $"Axis {(int)motion.Axis} {(motion.AxisValue < 0 ? "-" : "+")}"
        },
        _ => "Unbound"
    };
}
