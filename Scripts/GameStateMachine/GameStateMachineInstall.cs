namespace VGameFoundation.Script.GameStateMachine
{
     using System.Linq;
     using VContainer;
     using VGameFoundation.DI;
     using VGameFoundation.Scripts.Utilities.Extension;

     public static class GameStateMachineInstall
     {
          public static void RegisterGameStateMachine(this IContainerBuilder builder)
          {
               builder.Register<GameStateMachine>(Lifetime.Singleton).
                       AsImplementedInterfaces().
                       AsSelf().
                       WithParameter(container =>
                            typeof(IGameState).GetDerivedTypes().Select(type => (IState)container.Instantiate(type)).ToList());
          }
     }
}
