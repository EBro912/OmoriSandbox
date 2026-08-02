using Godot;
using System.Linq;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;
using OmoriSandbox.Modding;

namespace OmoriSandbox.Actors;

internal class ModdedEnemy : Enemy
{
    private JsonEnemyMod JsonEnemy;
    private SpriteFrames BuiltFrames;

    public ModdedEnemy(JsonEnemyMod jsonEnemy, SpriteFrames builtFrames)
    {
        JsonEnemy = jsonEnemy;
        BuiltFrames = builtFrames;
    }

    public override SpriteFrames Animation => BuiltFrames;

    public override string Name => JsonEnemy.Name.ToUpper();

    protected override Stats Stats => new(JsonEnemy.HP, JsonEnemy.Juice, JsonEnemy.ATK, JsonEnemy.DEF, JsonEnemy.SPD, JsonEnemy.LCK, JsonEnemy.HIT);

    protected override string[] EquippedSkills => JsonEnemy.EquippedSkills ?? [];

    public override bool IsEmotionValid(Emotion emotion)
    {
        return JsonEnemy.InvalidStates == null || !JsonEnemy.InvalidStates.Contains(emotion.Id);
    }

    public override BattleCommand ProcessAI()
    {
        if (JsonEnemy.AI == null)
        {
            GD.PrintErr($"Modded enemy {Name} has no AI!");
            return new BattleCommand(this, this, new EmptyAction());
        }
        
        JsonEnemyAIData data = JsonEnemy.AI.FirstOrDefault(x => x.Emotion == CurrentEmotion.Id);
        if (data.Equals(default(JsonEnemyAIData)))
        {
            GD.PrintErr($"Modded enemy {Name} is missing AI data for emotion {CurrentEmotion.Id}");
            return new BattleCommand(this, this, new EmptyAction());
        }
        
        PartyMember observed = ObserveTarget;
        bool observedAll = ObserveMultiTarget;
        ObserveTarget = null;
        ObserveMultiTarget = false;

        foreach (JsonEnemyAIEntry entry in data.Entries)
        {
            if (observedAll && entry.Skill == JsonEnemy.ObserveMultiSkill)
                if (TryUseSkill(entry, out BattleCommand command))
                    return command;

            if (observed != null && entry.Skill == JsonEnemy.ObserveSingleSkill)
                if (TryUseSkill(entry, out BattleCommand command, observed))
                    return command;

            if (Roll() <= entry.Chance)
                if (TryUseSkill(entry, out BattleCommand command))
                    return command;
        }
        GD.PrintErr($"Modded enemy {Name} ProcessAI failed due to an error.");
        return new BattleCommand(this, this, new EmptyAction());
    }

    private bool TryUseSkill(JsonEnemyAIEntry entry, out BattleCommand command, PartyMember observeTarget = null)
    {
        if (!Database.TryGetSkill(entry.Skill, out Skill skill))
        {
            GD.PrintErr($"Unknown skill {entry.Skill} for modded enemy {Name}!");
            command = null;
            return false;
        }
        if (!Skills.TryGetValue(entry.Skill, out skill))
        {
            GD.PrintErr($"Modded enemy {Name} does not have the {entry.Skill} skill equipped!");
            command = null;
            return false;
        }

        command = skill.Target switch
        {
            SkillTarget.Self => new BattleCommand(this, this, skill),
            SkillTarget.AllAllies => new BattleCommand(this, SelectAllEnemies(), skill),
            SkillTarget.AllEnemies => new BattleCommand(this, SelectAllTargets(), skill),
            SkillTarget.Ally or SkillTarget.AllyNotSelf => new BattleCommand(this, SelectEnemy(), skill),
            SkillTarget.Enemy or SkillTarget.AllyOrEnemy => new BattleCommand(this, observeTarget ?? SelectTarget(), skill),
            SkillTarget.XRandomEnemies when !entry.NumTargets.HasValue => null,
            SkillTarget.XRandomEnemies when entry.NumTargets.HasValue => new BattleCommand(this,
                SelectTargets(entry.NumTargets.Value), skill),
            _ => null
        };

        if (command == null)
        {
            GD.PrintErr($"{skill.Name} on Modded Enemy is either missing data or not supported.");
            return false;
        }

        return true;
    }
}