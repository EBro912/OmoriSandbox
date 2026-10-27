using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OmoriSandbox.Battle;

namespace OmoriSandbox.Actors;

internal static class GhostKids
{
    private static readonly ConditionalWeakTable<Enemy, BattleCommand> FellInCommand = new();

    private static bool IsGhostKid(Enemy enemy) => enemy is Salli or Cindi or Nanci or Lili or Dorothi or DorothiTrain or MerciAbyss or Angi or Merci;

    internal static bool AnotherAlive(Enemy self)
    {
        BattleCommand current = BattleManager.Instance.GetCurrentCommand();
        foreach (Enemy other in BattleManager.Instance.GetAllEnemies())
        {
            if (other == self || !IsGhostKid(other))
                continue;
            if (!other.IsToast && !other.HasEscaped)
                return true;
            if (current is { Forced: false } && FellInCommand.TryGetValue(other, out BattleCommand fell) && fell == current)
                return true;
        }
        return false;
    }

    internal static async Task Fall(Enemy self, string line)
    {
        if (BattleManager.Instance.AbortRequested)
            return;
        if (AnotherAlive(self))
        {
            FellInCommand.AddOrUpdate(self, BattleManager.Instance.GetCurrentCommand());
            self.RemoveImmortal();
            return;
        }
        if (line != null)
        {
            DialogueManager.Instance.QueueMessage(self, line, font: DialogueManager.FontType.Jagged);
            await DialogueManager.Instance.WaitForDialogue();
        }
        await self.AbortBattle();
    }
}
