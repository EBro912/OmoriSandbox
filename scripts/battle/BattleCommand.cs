public abstract class BattleCommand
{
    public Actor Actor;
    public Actor Target;
    public bool GoesFirst;

    public BattleCommand(Actor actor, Actor target, bool goesFirst = false)
    {
        Actor = actor;
        Target = target;
        GoesFirst = goesFirst;
    }

    public abstract void Run();
}
