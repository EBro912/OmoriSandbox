public class DoNothingCommand : BattleCommand
{
    public DoNothingCommand(Actor actor, string message = "") : base(actor, null, message) { }

    public override CommandResult Run()
    {
        GameManager.Instance.ClearAndMessageBattleLog(ParseMessage(Message));
        return new CommandResult(false);
    }
}