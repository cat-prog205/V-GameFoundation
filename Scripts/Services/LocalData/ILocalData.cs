namespace VGameFoundation.Scripts.Services.LocalData
{
    using System;
    using VContainer;
    using VGameFoundation.Scripts.Utilities.Extension;

    /// <summary>
    /// Storage key decoupled from the class name, so the class can be renamed or moved
    /// to another namespace later without losing already saved player data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class LocalDataKeyAttribute : Attribute
    {
        public string Key { get; }

        public LocalDataKeyAttribute(string key) => this.Key = key;
    }

    public interface ILocalData
    {
        /// <summary>
        /// Assigns the default value of every field. Must be idempotent.
        /// Always runs before the saved json is applied on top, so a field added in a later
        /// build gets a sane default instead of null/0 for players upgrading from an older build.
        /// </summary>
        void Init();

        void OnDataLoaded();
    }

    /// <summary>Local data whose schema may need upgrading between builds.</summary>
    public interface IMigratableLocalData : ILocalData
    {
        /// <summary>Version read back from json. Stamped by the framework, do not assign it in <see cref="ILocalData.Init"/>.</summary>
        int SavedVersion { get; set; }

        /// <summary>Version the current build expects. Bump it whenever the schema changes.</summary>
        int LatestVersion { get; }

        /// <summary>Called when <see cref="SavedVersion"/> is behind <see cref="LatestVersion"/>, before OnDataLoaded.</summary>
        void Migrate(int fromVersion);
    }

    public static class ILocalDataExtension
    {
        /// <param name="storageFactory">
        /// Overrides where saves are kept. Leave it out for encrypted local files; pass a factory
        /// to layer something else on top, such as a backend that also mirrors to Unity Cloud Save.
        /// </param>
        public static void RegisterLocalData(this IContainerBuilder builder, Func<IUserDataStorage> storageFactory = null)
        {
            foreach (Type type in typeof(ILocalData).GetDerivedTypes())
            {
                builder.Register(type, Lifetime.Singleton);
            }

            builder.Register(_ => storageFactory?.Invoke() ?? UserDataStorages.CreateDefault(), Lifetime.Singleton);
            builder.Register<UserDataManager>(Lifetime.Singleton);
            builder.Register<HandleUserDataServices>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
