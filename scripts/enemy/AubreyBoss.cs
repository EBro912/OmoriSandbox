using System.Collections.Generic;
using System.Linq;
using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Battle.Emotions;

namespace OmoriSandbox.Actors;

internal sealed class AubreyBoss : Enemy
{
    public override string Name => "AUBREY";
    public override SpriteFrames Animation => ResourceLoader.Load<SpriteFrames>("res://animations/aubrey_boss.tres");
    public override Vector2 InfoBoxOffset => new(0, -350);
    public override Vector2 DamageNumberOffset => new(0, -150);
    protected override Stats Stats => new(12000, 4000, 120, 55, 75, 15, 95);
    protected override string[] EquippedSkills => ["AAttack", "ABossBeatdown", "ABossLookAtKel", "ABossLookAtHero", "ABossLookAtKelAttack", "ABossLookAtHeroAttack", "PowerHit", "WindUpThrow", "MoodWrecker", "PepTalk", "ABossTwirl"];
    protected internal override bool ObserveHasMulti => false;
    public override bool IsEmotionValid(Emotion emotion)
    {
        return emotion.Id is "neutral" or "happy" or "sad" or "angry" or "afraid";
    }

    public override BattleCommand ProcessAI()
    {
        if (CanUse("WindUpThrow") && HasMultiTargetObserve())
            return new BattleCommand(this, SelectAllTargets(), Skills["WindUpThrow"]);
        if (CanUse("ABossBeatdown") && HasObserveTarget(out PartyMember observe))
            return new BattleCommand(this, observe, Skills["ABossBeatdown"]);

        if (CanUse("ABossBeatdown") && TrueTurnCount == 1)
            return new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]);

        if (CanUse("PepTalk") && TrueTurnCount == 3)
        {
            Enemy ally = SelectEnemyWithHighest(x => x.CurrentStats.SPD, x => x.CurrentEmotion.Id != "happy");
            if (ally != null)
                return new BattleCommand(this, ally, Skills["PepTalk"]);
        }

        IReadOnlyList<PartyMember> targets = SelectAllTargets();
        if (CanUse("ABossBeatdown"))
        {
            List<PartyMember> tickled = targets.Where(x => x.HasStatModifier("Tickle")).ToList();
            if (tickled.Count > 0)
                return new BattleCommand(this, tickled[GameManager.Instance.Random.RandiRange(0, tickled.Count - 1)], Skills["ABossBeatdown"]);
        }

        foreach (string tier in (string[])["happy", "ecstatic", "manic"])
        {
            if (!CanUse("MoodWrecker"))
                break;
            List<PartyMember> holders = targets.Where(x => x.CurrentEmotion.Id == tier).ToList();
            if (holders.Count > 0 && Roll() < 56)
                return new BattleCommand(this, holders[GameManager.Instance.Random.RandiRange(0, holders.Count - 1)], Skills["MoodWrecker"]);
        }

        return CurrentEmotion.Id switch
        {
            "happy" => ProcessHappy(),
            "sad" => ProcessSad(),
            "angry" => ProcessAngry(),
            _ => ProcessNeutral()
        };
    }

    private BattleCommand ProcessNeutral()
    {
        if (CanUse("AAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["AAttack"]);
        if (CanUse("ABossLookAtKelAttack") && SelectAllEnemies().Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["ABossLookAtKelAttack"]);
        if (CanUse("ABossLookAtHeroAttack") && SelectAllEnemies().Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["ABossLookAtHeroAttack"]);
        if (CanUse("PowerHit") && Roll() < 41)
            return new BattleCommand(this, SelectTargetWithHighest(StatType.DEF), Skills["PowerHit"]);
        if (CanUse("WindUpThrow") && Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["WindUpThrow"]);
        // VANILLA BUG: "Element HAPPY Weakness" always passes for untested party members, so TWIRL is used instead of BEATDOWN
        if (CanUse("ABossTwirl"))
            return new BattleCommand(this, SelectTarget(), Skills["ABossTwirl"]);
        return CanUse("ABossBeatdown") ? new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]) : null;
    }

    private BattleCommand ProcessHappy()
    {
        if (CanUse("AAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["AAttack"]);
        if (CanUse("PowerHit") && Roll() < 41)
            return new BattleCommand(this, SelectTargetWithHighest(StatType.DEF), Skills["PowerHit"]);
        if (CanUse("WindUpThrow") && Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["WindUpThrow"]);
        return CanUse("ABossBeatdown") ? new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]) : null;
    }

    private BattleCommand ProcessSad()
    {
        if (CanUse("ABossLookAtKelAttack") && SelectAllEnemies().Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["ABossLookAtKelAttack"]);
        if (CanUse("ABossLookAtHeroAttack") && SelectAllEnemies().Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["ABossLookAtHeroAttack"]);
        if (CanUse("PowerHit") && Roll() < 41)
            return new BattleCommand(this, SelectTargetWithHighest(StatType.DEF), Skills["PowerHit"]);
        if (CanUse("WindUpThrow") && Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["WindUpThrow"]);
        // VANILLA BUG: "Element HAPPY Weakness" always passes for untested party members, so TWIRL is used instead of BEATDOWN
        if (CanUse("ABossTwirl"))
            return new BattleCommand(this, SelectTarget(), Skills["ABossTwirl"]);
        return CanUse("ABossBeatdown") ? new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]) : null;
    }

    private BattleCommand ProcessAngry()
    {
        // VANILLA BUG: "Element HAPPY Weakness" always passes for untested party members, so ANGRY AUBREY always uses TWIRL
        if (CanUse("ABossTwirl"))
            return new BattleCommand(this, SelectTarget(), Skills["ABossTwirl"]);
        if (CanUse("AAttack") && Roll() < 21)
            return new BattleCommand(this, SelectTarget(), Skills["AAttack"]);
        if (CanUse("ABossLookAtHeroAttack") && SelectAllEnemies().Count > 2 && Roll() < 26)
            return new BattleCommand(this, SelectTarget(), Skills["ABossLookAtHeroAttack"]);
        if (CanUse("PowerHit") && Roll() < 41)
            return new BattleCommand(this, SelectTargetWithHighest(StatType.DEF), Skills["PowerHit"]);
        if (CanUse("WindUpThrow") && Roll() < 36)
            return new BattleCommand(this, SelectAllTargets(), Skills["WindUpThrow"]);
        return CanUse("ABossBeatdown") ? new BattleCommand(this, SelectTarget(), Skills["ABossBeatdown"]) : null;
    }
}