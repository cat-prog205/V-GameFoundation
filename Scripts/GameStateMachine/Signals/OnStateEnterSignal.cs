namespace VGameFoundation.Script.GameStateMachine
{
    public class OnStateEnterSignal
    {
        public IState State { get; }

        public OnStateEnterSignal(IState state)
        {
            this.State = state;
        }
    }
}