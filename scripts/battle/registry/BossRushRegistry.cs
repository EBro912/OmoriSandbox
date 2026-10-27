using System.Threading.Tasks;
using OmoriSandbox.Actors;
using OmoriSandbox.Animation;

namespace OmoriSandbox.Battle.Registry;

using static Database;

internal static class BossRushRegistry
{
	internal static void Register()
	{
		Enemies.Add("LostSproutMole (King Crawler)", () => new LostSproutMoleKC());
		Enemies.Add("Sweetheart (Boss Rush)", () => new SweetheartAlt());
		Enemies.Add("SlimeGirls (Boss Rush)", () => new SlimeGirlsAlt());
		Enemies.Add("DownloadWindow (Boss Rush)", () => new DownloadWindowAlt());
		Enemies.Add("SpaceExBoyfriend (Boss Rush)", () => new SpaceExBoyfriendAlt());
		Enemies.Add("GatorGuyJawsum (Boss Rush)", () => new GatorGuyJawsumAlt());
		Enemies.Add("MrJawsum (Boss Rush)", () => new MrJawsumAlt());
		Enemies.Add("YeOldSprout (Boss Rush)", () => new YeOldSproutAlt());
		Enemies.Add("SirMaximusI (Boss Rush)", () => new SirMaximusIAlt());
		Enemies.Add("SirMaximusII (Boss Rush)", () => new SirMaximusIIAlt());
		Enemies.Add("SirMaximusIII (Boss Rush)", () => new SirMaximusIIIAlt());
		Enemies.Add("PlutoExpanded (Boss Rush)", () => new PlutoExpandedAlt());
		Enemies.Add("KingCrawler (Boss Rush)", () => new KingCrawlerAlt());
		Enemies.Add("HumphreySwarm (Boss Rush)", () => new HumphreySwarmAlt());
		Enemies.Add("HumphreyGrande (Boss Rush)", () => new HumphreyGrandeAlt());
		Enemies.Add("HumphreyFace (Boss Rush)", () => new HumphreyFaceAlt());

		// Ye Old Sprout (Boss Rush) //
		Skills["YOSBRRollOver"] = new Skill(
			name: "YOSBRRollOver",
			description: "YOSBRRollOver",
			target: SkillTarget.AllEnemies,
			cost: 0,
			effect: async (self, targets) =>
			{
				BattleLogManager.Instance.QueueMessage(self, "[actor] rolls over!");
				foreach (Actor member in targets)
				{
					AnimationManager.Instance.PlayAnimation(124, member);
					BattleManager.Instance.Damage(self, member, () => 2 * self.CurrentStats.ATK - member.CurrentStats.DEF, false, neverCrit: true);
				}
				await Task.CompletedTask;
			}
		);
	}
}
