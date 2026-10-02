namespace VGameFoundation.Script.Services.CacheDataService
{
    using VContainer;
    using VGameFoundation.DI;
    using VGameFoundation.Scripts.Utilities.Extension;
    using VGameFoundation.Scripts.Utilities.LogService;

    public static class CachedDataRegister
    {
        public static void RegisterCachedData(this IContainerBuilder builder)
        {
            typeof(ICachedData).GetDerivedTypes().ForEach(type =>
            {
#if UNITY_EDITOR
                LogService.Log("create type cached data: " + type);
#endif
                builder.Register(type, Lifetime.Singleton).AsInterfacesAndSelf();
            });
        }
    }
}