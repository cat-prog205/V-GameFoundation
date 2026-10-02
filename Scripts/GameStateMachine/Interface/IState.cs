namespace VGameFoundation.Script.GameStateMachine
{
    public interface IState
    {
        void Enter();
        void Exit();
    }

    public interface IState<in TModel> : IState
    {
        public TModel Model { set; }
    }
}