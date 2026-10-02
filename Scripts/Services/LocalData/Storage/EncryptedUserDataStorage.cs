namespace VGameFoundation.Scripts.Services.LocalData
{
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using VGameFoundation.Scripts.Utilities.Data;

    /// <summary>
    /// Encrypts payloads on their way into any other storage. Applied as a wrapper rather than
    /// built into a backend so each backend can opt in separately: local files benefit from it,
    /// while a remote backend is usually better off storing readable json so the data stays
    /// inspectable and repairable server side.
    /// </summary>
    public sealed class EncryptedUserDataStorage : IUserDataStorage
    {
        private readonly IUserDataStorage inner;
        private readonly AesTextCipher    cipher;

        public EncryptedUserDataStorage(IUserDataStorage inner, string passphrase)
        {
            this.inner  = inner;
            this.cipher = new AesTextCipher(passphrase);
        }

        public async UniTask<string[][]> LoadAsync(params string[] keys)
        {
            var storedCandidates = await this.inner.LoadAsync(keys);

            // Candidates that fail to decrypt are dropped rather than surfaced, so the layer
            // above falls through to the next candidate or to defaults.
            return storedCandidates.Select(candidates => candidates.Select(this.Decrypt).Where(payload => payload != null).ToArray()).ToArray();
        }

        public UniTask SaveAsync(params (string key, string payload)[] entries)
        {
            return this.inner.SaveAsync(entries.Select(entry => (entry.key, this.cipher.Encrypt(entry.payload))).ToArray());
        }

        public UniTask DeleteAsync(params string[] keys)
        {
            return this.inner.DeleteAsync(keys);
        }

        private string Decrypt(string payload)
        {
            return this.cipher.TryDecrypt(payload, out var plainText) ? plainText : null;
        }
    }
}
