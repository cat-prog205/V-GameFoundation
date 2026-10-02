namespace VGameFoundation.Scripts.Services.LocalData
{
    using Newtonsoft.Json;

    /// <summary>
    /// Recommended base for every local data class: versioning is available from day one,
    /// so migration can be added later without touching already shipped saves.
    /// </summary>
    public abstract class BaseLocalData : IMigratableLocalData
    {
        public int SavedVersion { get; set; }

        [JsonIgnore] public virtual int LatestVersion => 1;

        public abstract void Init();

        public virtual void Migrate(int fromVersion) { }

        public virtual void OnDataLoaded() { }
    }
}
