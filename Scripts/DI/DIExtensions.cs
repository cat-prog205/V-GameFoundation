#nullable enable
namespace VGameFoundation.DI
{

    using UnityEngine;
    using VContainer;

    public static class DIExtensions
    {
        private static SceneScope? CurrentSceneContext;

        /// <summary>
        ///     Get current scene <see cref="IDependencyContainer"/>
        /// </summary>
        public static IDependencyContainer GetCurrentContainer()
        {
            if (CurrentSceneContext == null) CurrentSceneContext = Object.FindAnyObjectByType<SceneScope>();

            return CurrentSceneContext.Container.Resolve<IDependencyContainer>();
        }

        public static IObjectResolver GetCurrentObjectResolver(this object _)
        {
            if (CurrentSceneContext == null) CurrentSceneContext = Object.FindAnyObjectByType<SceneScope>();

            return CurrentSceneContext.Container;
        }

        /// <inheritdoc cref="GetCurrentContainer()"/>
        public static IDependencyContainer GetCurrentContainer(this object _) { return GetCurrentContainer(); }
    }
}