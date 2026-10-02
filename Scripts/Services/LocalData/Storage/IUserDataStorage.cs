namespace VGameFoundation.Scripts.Services.LocalData
{
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Where the bytes physically live. Deliberately knows nothing about json, data classes or
    /// versioning, which is what lets a remote backend such as Unity Cloud Save be dropped in
    /// beside the local one without touching any layer above it.
    /// </summary>
    public interface IUserDataStorage
    {
        /// <summary>
        /// The payloads stored for each requested key, newest first, in the same order as
        /// <paramref name="keys"/>. A key with nothing stored yields an empty array. Entries after
        /// the first are older copies the caller may fall back to when the newest one turns out
        /// to be unusable, so a backend without history simply returns a single entry.
        /// </summary>
        UniTask<string[][]> LoadAsync(params string[] keys);

        UniTask SaveAsync(params (string key, string payload)[] entries);

        UniTask DeleteAsync(params string[] keys);
    }
}
