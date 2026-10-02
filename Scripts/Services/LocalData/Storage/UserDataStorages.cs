namespace VGameFoundation.Scripts.Services.LocalData
{
    /// <summary>
    /// Single place the storage stack is assembled, so runtime and editor tooling always agree
    /// on where saves live and how they are encoded.
    /// </summary>
    public static class UserDataStorages
    {
        /// <summary>
        /// Changing this makes every existing save unreadable, so it must stay fixed for the
        /// lifetime of the game.
        /// </summary>
        private const string Passphrase = "cat@gamestudio";

        /// <summary>Encrypted json files under the persistent data path.</summary>
        public static IUserDataStorage CreateDefault()
        {
            return new EncryptedUserDataStorage(new FileUserDataStorage(), Passphrase);
        }

        /// <summary>
        /// The pre-file layout. Only useful for reading saves written by a build that shipped
        /// before the move to files.
        /// </summary>
        public static IUserDataStorage CreateLegacyPlayerPrefs()
        {
            return new EncryptedUserDataStorage(new PlayerPrefsUserDataStorage(), Passphrase);
        }
    }
}
