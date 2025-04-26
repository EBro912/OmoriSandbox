public class DoNothingCommand : BattleCommand
{
    private string Message;
    public DoNothingCommand(Actor actor, string message) : base(actor, null)
    {
        Message = message;
    }
    public override void Run()
    {
        GameManager.Instance.ClearAndMessageBattleLog(Message);
    }
}