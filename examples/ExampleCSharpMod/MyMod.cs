using Godot;
using OmoriSandbox.Modding;

namespace OmoriSandboxSampleMod
{
    public partial class MyMod : Mod
    {
        public override void OnLoad()
        {
            RegisterPartyMember<Tony>("Tony");

            GD.Print("MyMod loaded!");
        }
    }
}
