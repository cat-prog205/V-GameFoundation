namespace VGameFoundation.Script.GameStateMachine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VContainer.Unity;
    using VGameFoundation.Scripts.Utilities.LogService;
    using VGameFoundation.Signals;

    public abstract class StateMachine : IStateMachine, ITickable 
    {
        #region inject

        protected readonly ISignalBus                signalBus;
        protected readonly Dictionary<Type, IState> TypeToState;

        #endregion

        protected StateMachine(
            List<IState> listState,
            ISignalBus    signalBus
        )
        {
            this.signalBus   = signalBus;
            this.TypeToState = listState.ToDictionary(state => state.GetType(), state => state);
        }

        public IState CurrentState { get; private set; }

        public void TransitionTo<T>() where T : class, IState
        {
            this.TransitionTo(typeof(T));
        }

        public void TransitionTo<TState, TModel>(TModel model) where TState : class, IState<TModel>
        {
            var stateType = typeof(TState);
            if (!this.TypeToState.TryGetValue(stateType, out var nextState)) return;

            if (nextState is not TState nextStateT) return;
            nextStateT.Model = model;

            this.InternalStateTransition(nextState);
        }

        public virtual void TransitionTo(Type stateType)
        {
            if (!this.TypeToState.TryGetValue(stateType, out var nextState)) return;

            this.InternalStateTransition(nextState);
        }

        private void InternalStateTransition(IState nextState)
        {
            if (this.CurrentState != null)
            {
                this.CurrentState.Exit();
                this.signalBus.Fire(new OnStateExitSignal(this.CurrentState));
                LogService.Log($"Exit {this.CurrentState.GetType().Name} State!!!");
            }

            this.CurrentState = nextState;
            this.signalBus.Fire(new OnStateEnterSignal(this.CurrentState));
            LogService.Log( $"Enter {nextState.GetType().Name} State!!!");

            nextState.Enter();
        }

        public void Tick()
        {
            if (this.CurrentState is not ITickable tickableState) return;
            tickableState.Tick();
        }
    }
}