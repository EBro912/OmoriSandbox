using Godot;
using OmoriSandbox.Modding;

namespace OmoriSandbox.Actors;
internal class ModdedPartyMember : PartyMember
{
    private JsonActorMod JsonActor;
    private SpriteFrames BuiltFrames;

    public ModdedPartyMember(JsonActorMod jsonActor, SpriteFrames builtFrames)
    {
        JsonActor = jsonActor;
        BuiltFrames = builtFrames;
    }
    public override SpriteFrames Animation => BuiltFrames;

    public override int[] HPTree => JsonActor.HP;

    public override int[] JuiceTree => JsonActor.Juice;

    public override int[] ATKTree => JsonActor.ATK;

    public override int[] DEFTree => JsonActor.DEF;

    public override int[] SPDTree => JsonActor.SPD;

    public override int BaseLuck => JsonActor.LCK;

    public override string[] InvalidStates => JsonActor.InvalidStates;

    public override bool IsRealWorld => JsonActor.RealWorld;

    public override string Name => JsonActor.Name;

    public override bool HasPlotArmor => JsonActor.PlotArmor;
    public override string[] EquippableWeapons => JsonActor.EquippableWeapons;
}