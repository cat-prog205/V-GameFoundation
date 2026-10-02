namespace VGameFoundation.Script.GameStateMachine
{
    public class OnStateExitSignal
    {
        public IState State { get; }

        public OnStateExitSignal(IState state)
        {
            this.State = state;
        }
    }
}