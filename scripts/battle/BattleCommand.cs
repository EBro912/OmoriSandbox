public abstract class BattleCommand
{
    public Actor Actor;
    public bool GoesFirst;

    public BattleCommand(Actor actor, bool goesFirst = false)
    {
        Actor = actor;
        GoesFirst = goesFirst;
    }

    public abstract void Run();
}
