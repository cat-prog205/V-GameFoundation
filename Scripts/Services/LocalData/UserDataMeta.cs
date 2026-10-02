namespace VGameFoundation.Scripts.Services.LocalData
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Bookkeeping about the save itself rather than about gameplay. Stamped on every write so
    /// that a remote backend added later can decide which of two saves is newer, and whether the
    /// local one still holds changes that were never uploaded.
    /// </summary>
    /// <remarks>
    /// <see cref="Generation"/> answers "which save is newer" and is unrelated to
    /// <see cref="IMigratableLocalData.SavedVersion"/>, which answers "which schema is this".
    /// </remarks>
    [LocalDataKey("UserDataMeta")]
    public class UserDataMeta : BaseLocalData
    {
        /// <summary>Incremented on every write. The higher value wins when reconciling two saves.</summary>
        public long Generation;

        /// <summary>The generation last confirmed as uploaded. Zero until something has been synced.</summary>
        public long SyncedGeneration;

        public long SavedAtUtcTicks;

        /// <summary>Build that produced the save, useful when diagnosing a report from the field.</summary>
        public string AppVersion;

        [JsonIgnore] public DateTime SavedAtUtc => new(this.SavedAtUtcTicks, DateTimeKind.Utc);

        [JsonIgnore] public bool HasPendingUpload => this.Generation > this.SyncedGeneration;

        public override void Init()
        {
            this.Generation       = 0;
            this.SyncedGeneration = 0;
            this.SavedAtUtcTicks  = 0;
            this.AppVersion       = null;
        }
    }
}
