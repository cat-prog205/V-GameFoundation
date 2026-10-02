#nullable enable
namespace VGameFoundation.Signals
{
    using MessagePipe;
    using VContainer;
    using VGameFoundation.DI;

    public static class SignalBusVContainer
    {
        private static readonly MessagePipeOptions MessagePipeOptions = new();

        public static void RegisterSignalBus(this IContainerBuilder builder)
        {
            builder.Register<SignalBus>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.RegisterMessagePipe();
        }

        public static void DeclareSignal<TSignal>(this IContainerBuilder builder)
        {
            builder.RegisterMessageBroker<TSignal>(MessagePipeOptions);
        }
    }
}
