#nullable enable

namespace VGameFoundation.Scripts
{
    using GameFoundation.BlueprintFlow;
    using UnityEngine;
    using VContainer;
    using VGameFoundation.DI;
    using VGameFoundation.Script.GameStateMachine;
    using VGameFoundation.Script.Services.CacheDataService;
    using VGameFoundation.Script.Services.Localization;
    using VGameFoundation.Script.Services.MobiCommon.Vibration;
    using VGameFoundation.Script.SoundService;
    using VGameFoundation.Scripts.Services.GameAsset;
    using VGameFoundation.Scripts.Services.LocalData;
    using VGameFoundation.Scripts.Services.ObjectPooling;
    using VGameFoundation.Scripts.UI;
    using VGameFoundation.Signals;
    using VGameFoundation.Scripts.Services.TransitionService;

    public static class VGameFoundationContainer
    {
        public static void RegisterVGameFoundation(this IContainerBuilder builder, Transform rootTransform)
        {
            builder.Register<VContainerWrapper>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterSignalBus();
            builder.RegisterLocalData();
            builder.RegisterBlueprints();
            builder.RegisterGameStateMachine();

            builder.Register<ScreenManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GameAssets>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ObjectPoolManager>(Lifetime.Singleton);

            builder.Register<VibrationService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SoundService>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterCachedData();

#if Unity_Localization
            builder.Register<LocalizationService>(Lifetime.Singleton).AsInterfacesAndSelf();
#endif

#if Unity_Transition
            builder.Register<TransitionService>(Lifetime.Singleton).AsInterfacesAndSelf();
#endif
        }
    }
}