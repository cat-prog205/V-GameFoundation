namespace VGameFoundation.Scripts.Services.LocalData
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Scripting;
    using VGameFoundation.Scripts.Utilities.LogService;

    /// <summary>
    /// One file per key under <see cref="Application.persistentDataPath"/>.
    /// </summary>
    /// <remarks>
    /// Writes are synchronous on purpose. Local payloads are small enough that the cost is
    /// negligible, and it keeps <c>OnApplicationQuit</c> correct, where there is no opportunity
    /// to await anything. A remote backend is free to be genuinely asynchronous.
    /// </remarks>
    public sealed class FileUserDataStorage : IUserDataStorage
    {
        public const string DefaultFolderName = "UserData";

        private const string PayloadExtension = ".json";
        private const string BackupExtension  = ".bak";
        private const string TempExtension    = ".tmp";

        public static string DefaultRootPath => Path.Combine(Application.persistentDataPath, DefaultFolderName);

        private readonly string rootPath;

        public string RootPath => this.rootPath;

        [Preserve]
        public FileUserDataStorage() : this(DefaultRootPath)
        {
        }

        public FileUserDataStorage(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public UniTask<string[][]> LoadAsync(params string[] keys)
        {
            return UniTask.FromResult(keys.Select(this.ReadCandidates).ToArray());
        }

        public UniTask SaveAsync(params (string key, string payload)[] entries)
        {
            Directory.CreateDirectory(this.rootPath);

            foreach (var (key, payload) in entries)
            {
                try
                {
                    this.Write(this.PathOf(key), payload);
                }
                catch (Exception e)
                {
                    // A failing key must not abort the rest of the batch.
                    LogService.Exception(e, $"Failed to write user data '{key}'");
                }
            }

            return UniTask.CompletedTask;
        }

        public UniTask DeleteAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                var path = this.PathOf(key);
                Delete(path);
                Delete(path + BackupExtension);
                Delete(path + TempExtension);
            }

            return UniTask.CompletedTask;
        }

        private string[] ReadCandidates(string key)
        {
            var path = this.PathOf(key);

            return new[] { path, path + BackupExtension }
                .Select(Read)
                .Where(payload => !string.IsNullOrEmpty(payload))
                .ToArray();
        }

        private static string Read(string path)
        {
            try
            {
                return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
            }
            catch (Exception e)
            {
                LogService.Exception(e, $"Failed to read user data file '{path}'");
                return string.Empty;
            }
        }

        /// <summary>
        /// Writes through a temp file and demotes the current payload to the backup slot, so a
        /// process killed mid-write leaves either the previous payload or the backup readable,
        /// never a half written file.
        /// </summary>
        private void Write(string path, string payload)
        {
            var tempPath   = path + TempExtension;
            var backupPath = path + BackupExtension;

            File.WriteAllText(tempPath, payload, Encoding.UTF8);

            if (File.Exists(path))
            {
                Delete(backupPath);
                File.Move(path, backupPath);
            }

            File.Move(tempPath, path);
        }

        private static void Delete(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private string PathOf(string key)
        {
            return Path.Combine(this.rootPath, ToFileName(key) + PayloadExtension);
        }

        private static string ToFileName(string key)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var chars        = key.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalidChars, chars[i]) >= 0) chars[i] = '_';
            }

            return new string(chars);
        }
    }
}
