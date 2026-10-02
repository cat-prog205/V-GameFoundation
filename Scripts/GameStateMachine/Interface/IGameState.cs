namespace VGameFoundation.Script.GameStateMachine
{
    public interface IGameState : IState
    {
    }

    public interface IGameState<in TModel> : IState
    {
        public TModel Model { set; }
    }
}