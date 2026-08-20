using Godot;
using HarmonyLib;
using OmoriSandbox;
using OmoriSandbox.Actors;
using OmoriSandbox.Battle;
using OmoriSandbox.Modding;

namespace OmoriSandboxSampleMod
{
    public partial class MyMod : Mod
    {
        public override void OnLoad()
        {
            RegisterPartyMember<Tony>("Tony");

            // react to battle events without needing any patches
            BattleManager.Instance.BattleStarted += (_, args) =>
                GD.Print($"A battle started with preset {args.PresetName}!");
            BattleManager.Instance.DamageDealt += OnDamageDealt;

            // applies every [HarmonyPatch] class in this mod's assembly
            Harmony.PatchAll();

            GD.Print("MyMod loaded!");
        }

        private static void OnDamageDealt(object sender, DamageDealtEventArgs args)
        {
            if (args.Target is Tony)
                GD.Print($"Tony took {args.Damage} damage{(args.Critical ? " from a critical hit" : "")}!");
        }
    }

    // Example Harmony patch: Tony's attacks can never miss or be evaded.
    [HarmonyPatch(typeof(BattleManager), "RollMissOrEvade")]
    internal class TonyNeverMissesPatch
    {
        // the double underscore format here is a Harmony variable
        // in this case, we can override the result of the function
        // Prefix/Postfix functions also must be static and have the same parameters
        private static bool Prefix(Actor self, ref bool __result)
        {
            // important: only run the patch when the actor is actually Tony, to keep other actors functional
            if (self is not Tony)
                return true;

            // false means the attack was not a miss and was not evaded
            // returning false skips the original method, so no miss sound or message plays
            __result = false;
            return false;
        }
    }
}
