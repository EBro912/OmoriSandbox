public class DoNothingCommand : BattleCommand
{
    private string Message;
    public DoNothingCommand(Actor actor, string message) : base(actor)
    {
        Message = message;
    }
    public override void Run()
    {
        GameManager.Instance.ClearAndMessageBattleLog(Message);
    }
}