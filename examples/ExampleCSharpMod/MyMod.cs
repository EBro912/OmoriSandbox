using Godot;
using OmoriSandbox.Modding;

namespace OmoriSandboxSampleMod
{
    public partial class MyMod : Mod
    {
        public override void _Ready()
        {
            RegisterPartyMember<Tony>("Tony");

            GD.Print("MyMod loaded!");
        }
    }
}
