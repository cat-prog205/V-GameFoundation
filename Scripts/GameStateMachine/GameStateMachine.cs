namespace VGameFoundation.Script.GameStateMachine
{
     using System.Collections.Generic;
     using VGameFoundation.Signals;

     public class GameStateMachine : StateMachine
     {
          public GameStateMachine(List<IState> listState, ISignalBus signalBus) : base(listState, signalBus)
          {
               listState.ForEach(state =>
               {
                    if (state is IHaveStateMachine haveStateMachine) haveStateMachine.StateMachine = this;
               });
          }
     }
}
