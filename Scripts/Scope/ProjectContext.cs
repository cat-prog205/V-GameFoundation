namespace VGameFoundation.DI
{
    using VContainer;
    using VContainer.Unity;
    using VGameFoundation.Script.Services.ApplicationServices;
    using VGameFoundation.Script.Utilities;
    using VGameFoundation.Scripts;

    public class ProjectContext : SceneScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterVGameFoundation(this.transform);
            
            builder.RegisterComponentOnNewGameObject<MinimizeAppService>(Lifetime.Singleton).UnderTransform(this.transform);
            builder.AutoResolve<MinimizeAppService>();

#if UNITY_SHOW_FPS
            builder.RegisterComponentOnNewGameObject<Fps>(Lifetime.Singleton).UnderTransform(this.transform);
            builder.AutoResolve<Fps>();   
#endif

        }
    }
}

